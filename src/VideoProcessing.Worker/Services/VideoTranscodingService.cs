using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;
using VideoProcessing.Worker.Models;

namespace VideoProcessing.Worker.Services;

/// <summary>
/// Video transcoding service using FFmpeg
/// </summary>
public class VideoTranscodingService : IVideoTranscodingService
{
    private readonly ILogger<VideoTranscodingService> _logger;
    private readonly string _ffmpegPath;
    private readonly string _ffprobePath;

    public VideoTranscodingService(ILogger<VideoTranscodingService> logger, IConfiguration configuration)
    {
        _logger = logger;

        // FFmpeg paths - can be configured or use system PATH
        _ffmpegPath = configuration["FFmpeg:FFmpegPath"] ?? "ffmpeg";
        _ffprobePath = configuration["FFmpeg:FFprobePath"] ?? "ffprobe";

        // Set FFmpeg paths
        GlobalFFOptions.Configure(options =>
        {
            options.BinaryFolder = Path.GetDirectoryName(_ffmpegPath) ?? "/usr/bin";
            options.TemporaryFilesFolder = Path.GetTempPath();
        });
    }

    public async Task<VideoMetadataResult?> ExtractMetadataAsync(string inputFilePath)
    {
        try
        {
            _logger.LogInformation("Extracting metadata from {FilePath}", inputFilePath);

            var mediaInfo = await FFProbe.AnalyseAsync(inputFilePath);

            if (mediaInfo == null || mediaInfo.PrimaryVideoStream == null)
            {
                _logger.LogWarning("No video stream found in {FilePath}", inputFilePath);
                return null;
            }

            var videoStream = mediaInfo.PrimaryVideoStream;
            var audioStream = mediaInfo.PrimaryAudioStream;

            var aspectRatio = CalculateAspectRatio(videoStream.Width, videoStream.Height);

            var metadata = new VideoMetadataResult
            {
                DurationSeconds = (int)mediaInfo.Duration.TotalSeconds,
                Width = videoStream.Width,
                Height = videoStream.Height,
                AspectRatio = aspectRatio,
                FrameRate = videoStream.FrameRate,
                VideoCodec = videoStream.CodecName,
                VideoBitrate = (int)(videoStream.BitRate / 1000), // Convert to kbps
                AudioCodec = audioStream?.CodecName ?? "none",
                AudioBitrate = audioStream != null ? (int)(audioStream.BitRate / 1000) : 0,
                AudioChannels = audioStream?.Channels ?? 0,
                FileSize = new FileInfo(inputFilePath).Length
            };

            _logger.LogInformation("Metadata extracted: {Width}x{Height}, {Duration}s, {VideoCodec}/{AudioCodec}",
                metadata.Width, metadata.Height, metadata.DurationSeconds, metadata.VideoCodec, metadata.AudioCodec);

            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting metadata from {FilePath}", inputFilePath);
            return null;
        }
    }

    public async Task<string> TranscodeToQualityAsync(
        string inputFilePath,
        string outputFilePath,
        QualityPreset preset,
        IProgress<int>? progress = null)
    {
        try
        {
            _logger.LogInformation("Transcoding {Input} to {Quality} ({Width}x{Height})",
                inputFilePath, preset.Name, preset.Width, preset.Height);

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputFilePath);
            if (!string.IsNullOrEmpty(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Get source metadata to determine scaling
            var sourceMetadata = await ExtractMetadataAsync(inputFilePath);
            if (sourceMetadata == null)
            {
                throw new Exception("Failed to extract source video metadata");
            }

            // Don't upscale - if source is smaller than target, use source resolution
            var targetWidth = Math.Min(preset.Width, sourceMetadata.Width);
            var targetHeight = Math.Min(preset.Height, sourceMetadata.Height);

            // Ensure even dimensions (required for h264)
            targetWidth = targetWidth % 2 == 0 ? targetWidth : targetWidth - 1;
            targetHeight = targetHeight % 2 == 0 ? targetHeight : targetHeight - 1;

            var success = await FFMpegArguments
                .FromFileInput(inputFilePath)
                .OutputToFile(outputFilePath, true, options => options
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithAudioCodec(AudioCodec.Aac)
                    .WithVideoBitrate(preset.VideoBitrate)
                    .WithAudioBitrate(preset.AudioBitrate)
                    .WithVideoFilters(filterOptions => filterOptions
                        .Scale(targetWidth, targetHeight))
                    .WithFastStart()
                    .WithArgument(new CustomArgument("-preset fast"))
                    .WithArgument(new CustomArgument("-movflags +faststart"))
                    .WithArgument(new CustomArgument("-profile:v main"))
                )
                .NotifyOnProgress(percentage => progress?.Report((int)percentage),
                    TimeSpan.FromSeconds(sourceMetadata.DurationSeconds))
                .ProcessAsynchronously();

            if (!success)
            {
                throw new Exception($"FFmpeg transcoding failed for {preset.Name}");
            }

            _logger.LogInformation("Successfully transcoded to {Quality}: {OutputFile}",
                preset.Name, outputFilePath);

            return outputFilePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transcoding to {Quality}", preset.Name);
            throw;
        }
    }

    public async Task<string> GenerateHLSAsync(
        string inputFilePath,
        string outputDirectory,
        List<QualityPreset> presets,
        IProgress<int>? progress = null)
    {
        try
        {
            _logger.LogInformation("Generating HLS stream from {InputFile}", inputFilePath);

            Directory.CreateDirectory(outputDirectory);

            var sourceMetadata = await ExtractMetadataAsync(inputFilePath);
            if (sourceMetadata == null)
            {
                throw new Exception("Failed to extract source video metadata");
            }

            var masterPlaylistPath = Path.Combine(outputDirectory, "master.m3u8");
            var masterPlaylistLines = new List<string> { "#EXTM3U", "#EXT-X-VERSION:3" };

            // Generate HLS for each quality variant
            foreach (var preset in presets)
            {
                // Skip if preset resolution is higher than source
                if (preset.Width > sourceMetadata.Width)
                {
                    _logger.LogInformation("Skipping {Quality} - higher than source resolution", preset.Name);
                    continue;
                }

                var variantDir = Path.Combine(outputDirectory, preset.Name);
                Directory.CreateDirectory(variantDir);

                var playlistPath = Path.Combine(variantDir, "playlist.m3u8");
                var segmentPattern = Path.Combine(variantDir, "segment_%03d.ts");

                var targetWidth = Math.Min(preset.Width, sourceMetadata.Width);
                var targetHeight = Math.Min(preset.Height, sourceMetadata.Height);
                targetWidth = targetWidth % 2 == 0 ? targetWidth : targetWidth - 1;
                targetHeight = targetHeight % 2 == 0 ? targetHeight : targetHeight - 1;

                var success = await FFMpegArguments
                    .FromFileInput(inputFilePath)
                    .OutputToFile(playlistPath, true, options => options
                        .WithVideoCodec(VideoCodec.LibX264)
                        .WithAudioCodec(AudioCodec.Aac)
                        .WithVideoBitrate(preset.VideoBitrate)
                        .WithAudioBitrate(preset.AudioBitrate)
                        .WithVideoFilters(filterOptions => filterOptions
                            .Scale(targetWidth, targetHeight))
                        .WithArgument(new CustomArgument("-preset fast"))
                        .WithArgument(new CustomArgument("-hls_time 6"))
                        .WithArgument(new CustomArgument("-hls_list_size 0"))
                        .WithArgument(new CustomArgument($"-hls_segment_filename \"{segmentPattern}\""))
                        .ForceFormat("hls")
                    )
                    .NotifyOnProgress(percentage => progress?.Report((int)percentage / presets.Count),
                        TimeSpan.FromSeconds(sourceMetadata.DurationSeconds))
                    .ProcessAsynchronously();

                if (!success)
                {
                    _logger.LogWarning("Failed to generate HLS for {Quality}", preset.Name);
                    continue;
                }

                // Add variant to master playlist
                masterPlaylistLines.Add($"#EXT-X-STREAM-INF:BANDWIDTH={preset.VideoBitrate * 1000},RESOLUTION={targetWidth}x{targetHeight}");
                masterPlaylistLines.Add($"{preset.Name}/playlist.m3u8");

                _logger.LogInformation("Generated HLS variant: {Quality}", preset.Name);
            }

            // Write master playlist
            await File.WriteAllLinesAsync(masterPlaylistPath, masterPlaylistLines);

            _logger.LogInformation("HLS master playlist generated: {MasterPlaylist}", masterPlaylistPath);

            return masterPlaylistPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating HLS stream");
            throw;
        }
    }

    public async Task<List<string>> GenerateThumbnailsAsync(
        string inputFilePath,
        string outputDirectory,
        int count)
    {
        try
        {
            _logger.LogInformation("Generating {Count} thumbnails from {InputFile}", count, inputFilePath);

            Directory.CreateDirectory(outputDirectory);

            var metadata = await ExtractMetadataAsync(inputFilePath);
            if (metadata == null)
            {
                throw new Exception("Failed to extract video metadata for thumbnail generation");
            }

            var duration = metadata.DurationSeconds;
            var thumbnailPaths = new List<string>();

            // Generate thumbnails at evenly spaced intervals
            for (int i = 0; i < count; i++)
            {
                // Skip first and last 5 seconds to avoid black frames
                var skipStart = 5;
                var skipEnd = 5;
                var usableDuration = Math.Max(1, duration - skipStart - skipEnd);
                var timestamp = skipStart + (usableDuration * i / Math.Max(1, count - 1));

                var thumbnailPath = Path.Combine(outputDirectory, $"thumbnail_{i + 1}.jpg");

                var success = await FFMpegArguments
                    .FromFileInput(inputFilePath, true, options => options
                        .Seek(TimeSpan.FromSeconds(timestamp)))
                    .OutputToFile(thumbnailPath, true, options => options
                        .WithVideoFilters(filterOptions => filterOptions
                            .Scale(854, -1)) // Width 854px, maintain aspect ratio
                        .WithFrameOutputCount(1)
                        .ForceFormat("image2"))
                    .ProcessAsynchronously();

                if (success)
                {
                    thumbnailPaths.Add(thumbnailPath);
                    _logger.LogInformation("Generated thumbnail {Index} at {Timestamp}s: {Path}",
                        i + 1, timestamp, thumbnailPath);
                }
                else
                {
                    _logger.LogWarning("Failed to generate thumbnail {Index}", i + 1);
                }
            }

            return thumbnailPaths;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating thumbnails");
            throw;
        }
    }

    private string CalculateAspectRatio(int width, int height)
    {
        var gcd = GCD(width, height);
        var aspectWidth = width / gcd;
        var aspectHeight = height / gcd;

        // Common aspect ratios
        if (aspectWidth == 16 && aspectHeight == 9) return "16:9";
        if (aspectWidth == 4 && aspectHeight == 3) return "4:3";
        if (aspectWidth == 1 && aspectHeight == 1) return "1:1";
        if (aspectWidth == 9 && aspectHeight == 16) return "9:16";
        if (aspectWidth == 21 && aspectHeight == 9) return "21:9";

        return $"{aspectWidth}:{aspectHeight}";
    }

    private int GCD(int a, int b)
    {
        while (b != 0)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}

/// <summary>
/// Custom FFmpeg argument for advanced options
/// </summary>
public class CustomArgument : IArgument
{
    private readonly string _argument;

    public CustomArgument(string argument)
    {
        _argument = argument;
    }

    public string Text => _argument;
}
