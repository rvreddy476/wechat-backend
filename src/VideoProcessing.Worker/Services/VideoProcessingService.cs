using System.Diagnostics;
using VideoProcessing.Worker.Models;

namespace VideoProcessing.Worker.Services;

/// <summary>
/// Main service for orchestrating video processing workflow
/// </summary>
public class VideoProcessingService
{
    private readonly IVideoTranscodingService _transcodingService;
    private readonly IStorageService _storageService;
    private readonly IVideoRepository _videoRepository;
    private readonly ILogger<VideoProcessingService> _logger;
    private readonly string _tempDirectory;

    public VideoProcessingService(
        IVideoTranscodingService transcodingService,
        IStorageService storageService,
        IVideoRepository videoRepository,
        ILogger<VideoProcessingService> logger,
        IConfiguration configuration)
    {
        _transcodingService = transcodingService;
        _storageService = storageService;
        _videoRepository = videoRepository;
        _logger = logger;

        _tempDirectory = configuration["Processing:TempDirectory"] ?? Path.GetTempPath();
        Directory.CreateDirectory(_tempDirectory);
    }

    public async Task<VideoProcessingResult> ProcessVideoAsync(VideoProcessingJob job, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new VideoProcessingResult
        {
            VideoId = job.VideoId,
            Success = false
        };

        string? workingDirectory = null;
        string? downloadedFilePath = null;

        try
        {
            _logger.LogInformation("Starting video processing for {VideoId}", job.VideoId);

            // Update status to Processing
            await _videoRepository.UpdateProcessingStatusAsync(job.VideoId, "Processing", 0);

            // Create working directory
            workingDirectory = Path.Combine(_tempDirectory, $"video_{job.VideoId}_{Guid.NewGuid():N}");
            Directory.CreateDirectory(workingDirectory);

            // Step 1: Download source video (10% progress)
            _logger.LogInformation("Step 1/5: Downloading source video from {SourceUrl}", job.SourceUrl);
            downloadedFilePath = await DownloadSourceVideoAsync(job.SourceUrl, workingDirectory, job.OriginalFileName);
            await _videoRepository.UpdateProcessingStatusAsync(job.VideoId, "Processing", 10);

            // Step 2: Extract metadata (20% progress)
            _logger.LogInformation("Step 2/5: Extracting video metadata");
            var metadata = await _transcodingService.ExtractMetadataAsync(downloadedFilePath);
            if (metadata == null)
            {
                throw new Exception("Failed to extract video metadata");
            }
            result.Metadata = metadata;
            await _videoRepository.UpdateVideoMetadataAsync(job.VideoId, result);
            await _videoRepository.UpdateProcessingStatusAsync(job.VideoId, "Processing", 20);

            // Step 3: Generate thumbnails (30% progress)
            _logger.LogInformation("Step 3/5: Generating {Count} thumbnails", job.Options.ThumbnailCount);
            var thumbnailsDirectory = Path.Combine(workingDirectory, "thumbnails");
            var thumbnailPaths = await _transcodingService.GenerateThumbnailsAsync(
                downloadedFilePath,
                thumbnailsDirectory,
                job.Options.ThumbnailCount);

            // Upload thumbnails
            var thumbnailUrls = new List<string>();
            for (int i = 0; i < thumbnailPaths.Count; i++)
            {
                var thumbnailUrl = await _storageService.UploadFileAsync(
                    thumbnailPaths[i],
                    $"videos/{job.UserId}/{job.VideoId}/thumbnails/thumbnail_{i + 1}.jpg",
                    "image/jpeg");
                thumbnailUrls.Add(thumbnailUrl);
            }
            result.ThumbnailUrls = thumbnailUrls;
            await _videoRepository.UpdateProcessingStatusAsync(job.VideoId, "Processing", 30);

            // Step 4: Generate quality variants and HLS (30% - 90% progress)
            if (job.Options.GenerateHLS)
            {
                _logger.LogInformation("Step 4/5: Generating HLS streaming format");

                var hlsDirectory = Path.Combine(workingDirectory, "hls");
                var progress = new Progress<int>(percent =>
                {
                    var overallProgress = 30 + (int)(percent * 0.6); // 30% to 90%
                    _videoRepository.UpdateProcessingStatusAsync(job.VideoId, "Processing", overallProgress).Wait();
                });

                await _transcodingService.GenerateHLSAsync(
                    downloadedFilePath,
                    hlsDirectory,
                    job.Options.QualityPresets,
                    progress);

                // Upload HLS directory
                var streamingUrl = await _storageService.UploadDirectoryAsync(
                    hlsDirectory,
                    $"videos/{job.UserId}/{job.VideoId}/hls");

                result.StreamingUrl = streamingUrl;
            }
            else if (job.Options.GenerateQualityVariants)
            {
                _logger.LogInformation("Step 4/5: Generating quality variants");

                var qualityVariants = new List<QualityVariantResult>();
                var variantsDirectory = Path.Combine(workingDirectory, "variants");
                Directory.CreateDirectory(variantsDirectory);

                for (int i = 0; i < job.Options.QualityPresets.Count; i++)
                {
                    var preset = job.Options.QualityPresets[i];

                    // Skip if preset is higher resolution than source
                    if (preset.Width > metadata.Width)
                    {
                        _logger.LogInformation("Skipping {Quality} - higher than source resolution", preset.Name);
                        continue;
                    }

                    var outputPath = Path.Combine(variantsDirectory, $"{preset.Name}.mp4");

                    var progress = new Progress<int>(percent =>
                    {
                        var overallProgress = 30 + (int)((i + percent / 100.0) / job.Options.QualityPresets.Count * 60);
                        _videoRepository.UpdateProcessingStatusAsync(job.VideoId, "Processing", overallProgress).Wait();
                    });

                    await _transcodingService.TranscodeToQualityAsync(
                        downloadedFilePath,
                        outputPath,
                        preset,
                        progress);

                    // Upload variant
                    var variantUrl = await _storageService.UploadFileAsync(
                        outputPath,
                        $"videos/{job.UserId}/{job.VideoId}/variants/{preset.Name}.mp4",
                        "video/mp4");

                    var fileInfo = new FileInfo(outputPath);
                    qualityVariants.Add(new QualityVariantResult
                    {
                        Quality = preset.Name,
                        Url = variantUrl,
                        FileSize = fileInfo.Length,
                        Bitrate = preset.VideoBitrate,
                        Codec = job.Options.TargetCodec
                    });
                }

                result.QualityVariants = qualityVariants;

                // Use highest quality as streaming URL
                if (qualityVariants.Any())
                {
                    result.StreamingUrl = qualityVariants.OrderByDescending(v => v.Bitrate).First().Url;
                }
            }

            await _videoRepository.UpdateProcessingStatusAsync(job.VideoId, "Processing", 90);

            // Step 5: Finalize (100% progress)
            _logger.LogInformation("Step 5/5: Finalizing video processing");
            await _videoRepository.MarkAsReadyAsync(job.VideoId, result);
            await _videoRepository.UpdateProcessingStatusAsync(job.VideoId, "Ready", 100);

            result.Success = true;
            result.ProcessingTime = stopwatch.Elapsed;

            _logger.LogInformation("Successfully completed video processing for {VideoId} in {Duration}",
                job.VideoId, stopwatch.Elapsed);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing video {VideoId}", job.VideoId);

            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.ProcessingTime = stopwatch.Elapsed;

            await _videoRepository.MarkAsFailedAsync(job.VideoId, ex.Message);

            return result;
        }
        finally
        {
            // Cleanup: Delete temporary files
            if (!string.IsNullOrEmpty(workingDirectory) && Directory.Exists(workingDirectory))
            {
                try
                {
                    Directory.Delete(workingDirectory, true);
                    _logger.LogInformation("Cleaned up working directory: {WorkingDirectory}", workingDirectory);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cleanup working directory: {WorkingDirectory}", workingDirectory);
                }
            }
        }
    }

    private async Task<string> DownloadSourceVideoAsync(string sourceUrl, string workingDirectory, string originalFileName)
    {
        try
        {
            var fileName = Path.GetFileName(originalFileName);
            var downloadPath = Path.Combine(workingDirectory, $"source_{fileName}");

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(30); // Large file download timeout

            _logger.LogInformation("Downloading video from {SourceUrl}", sourceUrl);

            using var response = await httpClient.GetAsync(sourceUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fileStream);

            _logger.LogInformation("Downloaded video to {FilePath} ({FileSize} bytes)",
                downloadPath, new FileInfo(downloadPath).Length);

            return downloadPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading source video from {SourceUrl}", sourceUrl);
            throw;
        }
    }
}
