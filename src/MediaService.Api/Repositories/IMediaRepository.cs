using MediaService.Api.Models;
using Shared.Domain.Common;

namespace MediaService.Api.Repositories;

public interface IMediaRepository
{
    // Media Management
    Task<Result<Media>> CreateMediaAsync(Media media);
    Task<Result<Media>> GetMediaByIdAsync(string mediaId);
    Task<Result<List<Media>>> GetUserMediaAsync(Guid userId, MediaType? type = null, int page = 1, int pageSize = 20);
    Task<Result<bool>> UpdateMediaAsync(string mediaId, Media media);
    Task<Result<bool>> DeleteMediaAsync(string mediaId, Guid userId);
    Task<Result<bool>> UpdateMediaStatusAsync(string mediaId, MediaStatus status);
    Task<Result<bool>> UpdateMediaUrlsAsync(string mediaId, string url, string? cdnUrl = null, string? thumbnailUrl = null);

    // Upload Management (for chunked/resumable uploads)
    Task<Result<Upload>> CreateUploadAsync(Upload upload);
    Task<Result<Upload>> GetUploadByKeyAsync(string uploadKey);
    Task<Result<bool>> UpdateUploadProgressAsync(string uploadKey, int chunkNumber);
    Task<Result<bool>> CompleteUploadAsync(string uploadKey, string mediaId);
    Task<Result<bool>> FailUploadAsync(string uploadKey, string error);
    Task<Result<bool>> CleanupExpiredUploadsAsync();

    // Processing Jobs
    Task<Result<MediaProcessingJob>> CreateProcessingJobAsync(MediaProcessingJob job);
    Task<Result<MediaProcessingJob>> GetProcessingJobByIdAsync(string jobId);
    Task<Result<List<MediaProcessingJob>>> GetPendingJobsAsync(int limit = 10);
    Task<Result<bool>> UpdateJobProgressAsync(string jobId, ProcessingStatus status, int progress, string? error = null);
    Task<Result<bool>> CompleteJobAsync(string jobId);
    Task<Result<bool>> FailJobAsync(string jobId, string error);

    // Search and Stats
    Task<Result<List<Media>>> SearchMediaAsync(string query, MediaType? type = null, int page = 1, int pageSize = 20);
    Task<Result<long>> GetUserStorageUsedAsync(Guid userId);
    Task<Result<int>> GetUserMediaCountAsync(Guid userId, MediaType? type = null);
}
