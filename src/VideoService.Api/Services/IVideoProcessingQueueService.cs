using VideoService.Api.Models;

namespace VideoService.Api.Services;

/// <summary>
/// Service for enqueuing video processing jobs
/// </summary>
public interface IVideoProcessingQueueService
{
    /// <summary>
    /// Enqueue a video for processing
    /// </summary>
    Task EnqueueVideoProcessingAsync(Video video, string sourceUrl);

    /// <summary>
    /// Get current queue size
    /// </summary>
    Task<long> GetQueueSizeAsync();
}
