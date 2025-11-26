using StackExchange.Redis;
using System.Text.Json;
using VideoProcessing.Worker.Models;

namespace VideoProcessing.Worker.Services;

/// <summary>
/// Redis-based job queue for video processing
/// </summary>
public class RedisJobQueueService : IJobQueueService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisJobQueueService> _logger;
    private const string QueueKey = "video:processing:queue";
    private const string ProcessingKey = "video:processing:active";

    public RedisJobQueueService(ILogger<RedisJobQueueService> logger, IConfiguration configuration)
    {
        _logger = logger;

        var redisConnection = configuration["Redis:ConnectionString"]
            ?? throw new ArgumentNullException("Redis:ConnectionString configuration is required");

        _redis = ConnectionMultiplexer.Connect(redisConnection);
        _logger.LogInformation("Redis job queue initialized");
    }

    public async Task<VideoProcessingJob?> DequeueJobAsync(CancellationToken cancellationToken)
    {
        try
        {
            var db = _redis.GetDatabase();

            // Use BRPOPLPUSH for reliable queue processing (blocking pop with atomic move to processing set)
            var result = await db.ListRightPopLeftPushAsync(QueueKey, ProcessingKey);

            if (result.IsNullOrEmpty)
            {
                return null;
            }

            var job = JsonSerializer.Deserialize<VideoProcessingJob>(result.ToString());

            if (job != null)
            {
                _logger.LogInformation("Dequeued job for video {VideoId}", job.VideoId);
            }

            return job;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error dequeuing job from Redis");
            return null;
        }
    }

    public async Task RequeueJobAsync(VideoProcessingJob job)
    {
        try
        {
            var db = _redis.GetDatabase();

            // Increment retry count
            job.RetryCount++;

            if (job.RetryCount < job.MaxRetries)
            {
                var jobJson = JsonSerializer.Serialize(job);

                // Push back to queue for retry
                await db.ListLeftPushAsync(QueueKey, jobJson);

                _logger.LogInformation("Requeued job for video {VideoId} (retry {RetryCount}/{MaxRetries})",
                    job.VideoId, job.RetryCount, job.MaxRetries);
            }
            else
            {
                _logger.LogWarning("Job for video {VideoId} exceeded max retries ({MaxRetries})",
                    job.VideoId, job.MaxRetries);

                // Remove from processing set
                var jobJson = JsonSerializer.Serialize(job);
                await db.ListRemoveAsync(ProcessingKey, jobJson);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requeuing job for video {VideoId}", job.VideoId);
            throw;
        }
    }

    public async Task MarkJobCompletedAsync(string videoId)
    {
        try
        {
            var db = _redis.GetDatabase();

            // Get all items in processing set to find and remove this job
            var processingJobs = await db.ListRangeAsync(ProcessingKey);

            foreach (var item in processingJobs)
            {
                var job = JsonSerializer.Deserialize<VideoProcessingJob>(item.ToString());
                if (job?.VideoId == videoId)
                {
                    await db.ListRemoveAsync(ProcessingKey, item);
                    _logger.LogInformation("Marked job for video {VideoId} as completed", videoId);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking job as completed for video {VideoId}", videoId);
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
