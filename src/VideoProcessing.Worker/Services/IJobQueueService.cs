using VideoProcessing.Worker.Models;

namespace VideoProcessing.Worker.Services;

/// <summary>
/// Service for managing video processing job queue
/// </summary>
public interface IJobQueueService
{
    /// <summary>
    /// Dequeue next video processing job
    /// </summary>
    Task<VideoProcessingJob?> DequeueJobAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Re-enqueue a failed job for retry
    /// </summary>
    Task RequeueJobAsync(VideoProcessingJob job);

    /// <summary>
    /// Mark job as completed
    /// </summary>
    Task MarkJobCompletedAsync(string videoId);

    /// <summary>
    /// Get queue size
    /// </summary>
    Task<long> GetQueueSizeAsync();
}
