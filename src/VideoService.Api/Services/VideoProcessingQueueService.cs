using StackExchange.Redis;
using System.Text.Json;
using VideoService.Api.Models;

namespace VideoService.Api.Services;

/// <summary>
/// Redis-based service for enqueuing video processing jobs
/// </summary>
public class VideoProcessingQueueService : IVideoProcessingQueueService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<VideoProcessingQueueService> _logger;
    private const string QueueKey = "video:processing:queue";

    public VideoProcessingQueueService(
        IConnectionMultiplexer redis,
        ILogger<VideoProcessingQueueService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task EnqueueVideoProcessingAsync(Video video, string sourceUrl)
    {
        try
        {
            var job = new VideoProcessingJob
            {
                VideoId = video.Id,
                UserId = video.UserId,
                SourceUrl = sourceUrl,
                OriginalFileName = video.OriginalFileName,
                Options = new VideoProcessingOptions
                {
                    GenerateQualityVariants = true,
                    GenerateHLS = true,
                    ThumbnailCount = 5,
                    ExtractMetadata = true,
                    TargetCodec = "h264",
                    TargetAudioCodec = "aac"
                }
            };

            var jobJson = JsonSerializer.Serialize(job);

            var db = _redis.GetDatabase();
            await db.ListLeftPushAsync(QueueKey, jobJson);

            _logger.LogInformation("Enqueued video processing job for video {VideoId} (User: {UserId})",
                video.Id, video.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enqueuing video processing job for video {VideoId}", video.Id);
            throw;
        }
    }

    public async Task<long> GetQueueSizeAsync()
    {
        try
        {
            var db = _redis.GetDatabase();
            return await db.ListLengthAsync(QueueKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting queue size");
            return 0;
        }
    }
}

/// <summary>
/// Video processing job model (matches VideoProcessing.Worker model)
/// </summary>
public class VideoProcessingJob
{
    public string VideoId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public VideoProcessingOptions Options { get; set; } = new();
    public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// Processing options (matches VideoProcessing.Worker model)
/// </summary>
public class VideoProcessingOptions
{
    public bool GenerateQualityVariants { get; set; } = true;
    public bool GenerateHLS { get; set; } = true;
    public int ThumbnailCount { get; set; } = 5;
    public bool ExtractMetadata { get; set; } = true;
    public string TargetCodec { get; set; } = "h264";
    public string TargetAudioCodec { get; set; } = "aac";
}
