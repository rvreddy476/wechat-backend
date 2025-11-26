using VideoProcessing.Worker.Models;

namespace VideoProcessing.Worker.Services;

/// <summary>
/// Repository for updating video status in MongoDB
/// </summary>
public interface IVideoRepository
{
    /// <summary>
    /// Update video processing status
    /// </summary>
    Task UpdateProcessingStatusAsync(string videoId, string status, int progress, string? error = null);

    /// <summary>
    /// Update video metadata after processing
    /// </summary>
    Task UpdateVideoMetadataAsync(string videoId, VideoProcessingResult result);

    /// <summary>
    /// Mark video as ready for streaming
    /// </summary>
    Task MarkAsReadyAsync(string videoId, VideoProcessingResult result);

    /// <summary>
    /// Mark video processing as failed
    /// </summary>
    Task MarkAsFailedAsync(string videoId, string errorMessage);
}
