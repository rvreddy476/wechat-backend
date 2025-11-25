using MongoDB.Driver;
using MediaService.Api.Models;
using Shared.Domain.Common;
using Shared.Infrastructure.MongoDB;

namespace MediaService.Api.Repositories;

public class MediaRepository : MongoRepository<Media>, IMediaRepository
{
    private readonly IMongoCollection<Upload> _uploadsCollection;
    private readonly IMongoCollection<MediaProcessingJob> _jobsCollection;
    private readonly ILogger<MediaRepository> _logger;

    public MediaRepository(
        IMongoDatabase database,
        ILogger<MediaRepository> logger) : base(database, "media")
    {
        _uploadsCollection = database.GetCollection<Upload>("uploads");
        _jobsCollection = database.GetCollection<MediaProcessingJob>("processingJobs");
        _logger = logger;
    }

    // Media Management
    public async Task<Result<Media>> CreateMediaAsync(Media media)
    {
        try
        {
            media.CreatedAt = DateTime.UtcNow;
            media.UpdatedAt = DateTime.UtcNow;
            media.UploadedAt = DateTime.UtcNow;

            await Collection.InsertOneAsync(media);
            _logger.LogInformation("Created media {MediaId} for user {UserId}", media.Id, media.UserId);
            return Result<Media>.Success(media);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating media for user {UserId}", media.UserId);
            return Result<Media>.Failure($"Failed to create media: {ex.Message}");
        }
    }

    public async Task<Result<Media>> GetMediaByIdAsync(string mediaId)
    {
        try
        {
            var media = await Collection
                .Find(m => m.Id == mediaId && m.Status != MediaStatus.Deleted)
                .FirstOrDefaultAsync();

            if (media == null)
            {
                return Result<Media>.Failure("Media not found");
            }

            return Result<Media>.Success(media);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media {MediaId}", mediaId);
            return Result<Media>.Failure($"Failed to get media: {ex.Message}");
        }
    }

    public async Task<Result<List<Media>>> GetUserMediaAsync(Guid userId, MediaType? type = null, int page = 1, int pageSize = 20)
    {
        try
        {
            var filterBuilder = Builders<Media>.Filter;
            var filter = filterBuilder.Eq(m => m.UserId, userId) &
                         filterBuilder.Ne(m => m.Status, MediaStatus.Deleted);

            if (type.HasValue)
            {
                filter &= filterBuilder.Eq(m => m.MediaType, type.Value);
            }

            var media = await Collection
                .Find(filter)
                .SortByDescending(m => m.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Media>>.Success(media);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media for user {UserId}", userId);
            return Result<List<Media>>.Failure($"Failed to get user media: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateMediaAsync(string mediaId, Media media)
    {
        try
        {
            media.UpdatedAt = DateTime.UtcNow;

            var result = await Collection.ReplaceOneAsync(
                m => m.Id == mediaId,
                media);

            if (result.MatchedCount == 0)
            {
                return Result<bool>.Failure("Media not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating media {MediaId}", mediaId);
            return Result<bool>.Failure($"Failed to update media: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteMediaAsync(string mediaId, Guid userId)
    {
        try
        {
            var filter = Builders<Media>.Filter.Eq(m => m.Id, mediaId) &
                         Builders<Media>.Filter.Eq(m => m.UserId, userId);

            var update = Builders<Media>.Update
                .Set(m => m.Status, MediaStatus.Deleted)
                .Set(m => m.UpdatedAt, DateTime.UtcNow);

            var result = await Collection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                return Result<bool>.Failure("Media not found or unauthorized");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting media {MediaId}", mediaId);
            return Result<bool>.Failure($"Failed to delete media: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateMediaStatusAsync(string mediaId, MediaStatus status)
    {
        try
        {
            var update = Builders<Media>.Update
                .Set(m => m.Status, status)
                .Set(m => m.UpdatedAt, DateTime.UtcNow);

            if (status == MediaStatus.Ready)
            {
                update = update.Set(m => m.ProcessedAt, DateTime.UtcNow);
            }

            var result = await Collection.UpdateOneAsync(m => m.Id == mediaId, update);

            if (result.MatchedCount == 0)
            {
                return Result<bool>.Failure("Media not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating media status {MediaId}", mediaId);
            return Result<bool>.Failure($"Failed to update media status: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateMediaUrlsAsync(string mediaId, string url, string? cdnUrl = null, string? thumbnailUrl = null)
    {
        try
        {
            var updateBuilder = Builders<Media>.Update
                .Set(m => m.Url, url)
                .Set(m => m.UpdatedAt, DateTime.UtcNow);

            if (!string.IsNullOrEmpty(cdnUrl))
            {
                updateBuilder = updateBuilder.Set(m => m.CdnUrl, cdnUrl);
            }

            if (!string.IsNullOrEmpty(thumbnailUrl))
            {
                updateBuilder = updateBuilder.Set(m => m.ThumbnailUrl, thumbnailUrl);
            }

            var result = await Collection.UpdateOneAsync(m => m.Id == mediaId, updateBuilder);

            if (result.MatchedCount == 0)
            {
                return Result<bool>.Failure("Media not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating media URLs {MediaId}", mediaId);
            return Result<bool>.Failure($"Failed to update media URLs: {ex.Message}");
        }
    }

    // Upload Management
    public async Task<Result<Upload>> CreateUploadAsync(Upload upload)
    {
        try
        {
            upload.CreatedAt = DateTime.UtcNow;
            upload.UpdatedAt = DateTime.UtcNow;

            await _uploadsCollection.InsertOneAsync(upload);
            _logger.LogInformation("Created upload {UploadKey} for user {UserId}", upload.UploadKey, upload.UserId);
            return Result<Upload>.Success(upload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating upload for user {UserId}", upload.UserId);
            return Result<Upload>.Failure($"Failed to create upload: {ex.Message}");
        }
    }

    public async Task<Result<Upload>> GetUploadByKeyAsync(string uploadKey)
    {
        try
        {
            var upload = await _uploadsCollection
                .Find(u => u.UploadKey == uploadKey)
                .FirstOrDefaultAsync();

            if (upload == null)
            {
                return Result<Upload>.Failure("Upload not found");
            }

            return Result<Upload>.Success(upload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting upload {UploadKey}", uploadKey);
            return Result<Upload>.Failure($"Failed to get upload: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateUploadProgressAsync(string uploadKey, int chunkNumber)
    {
        try
        {
            var filter = Builders<Upload>.Filter.Eq(u => u.UploadKey, uploadKey);
            var update = Builders<Upload>.Update
                .AddToSet(u => u.UploadedChunks, chunkNumber)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _uploadsCollection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                return Result<bool>.Failure("Upload not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating upload progress {UploadKey}", uploadKey);
            return Result<bool>.Failure($"Failed to update upload progress: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CompleteUploadAsync(string uploadKey, string mediaId)
    {
        try
        {
            var filter = Builders<Upload>.Filter.Eq(u => u.UploadKey, uploadKey);
            var update = Builders<Upload>.Update
                .Set(u => u.Status, UploadStatus.Completed)
                .Set(u => u.MediaId, mediaId)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _uploadsCollection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                return Result<bool>.Failure("Upload not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing upload {UploadKey}", uploadKey);
            return Result<bool>.Failure($"Failed to complete upload: {ex.Message}");
        }
    }

    public async Task<Result<bool>> FailUploadAsync(string uploadKey, string error)
    {
        try
        {
            var filter = Builders<Upload>.Filter.Eq(u => u.UploadKey, uploadKey);
            var update = Builders<Upload>.Update
                .Set(u => u.Status, UploadStatus.Failed)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _uploadsCollection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                return Result<bool>.Failure("Upload not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error failing upload {UploadKey}", uploadKey);
            return Result<bool>.Failure($"Failed to fail upload: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CleanupExpiredUploadsAsync()
    {
        try
        {
            var filter = Builders<Upload>.Filter.And(
                Builders<Upload>.Filter.Lt(u => u.ExpiresAt, DateTime.UtcNow),
                Builders<Upload>.Filter.Ne(u => u.Status, UploadStatus.Completed)
            );

            var update = Builders<Upload>.Update
                .Set(u => u.Status, UploadStatus.Expired)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _uploadsCollection.UpdateManyAsync(filter, update);

            _logger.LogInformation("Marked {Count} uploads as expired", result.ModifiedCount);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired uploads");
            return Result<bool>.Failure($"Failed to cleanup expired uploads: {ex.Message}");
        }
    }

    // Processing Jobs
    public async Task<Result<MediaProcessingJob>> CreateProcessingJobAsync(MediaProcessingJob job)
    {
        try
        {
            job.CreatedAt = DateTime.UtcNow;
            job.UpdatedAt = DateTime.UtcNow;

            await _jobsCollection.InsertOneAsync(job);
            _logger.LogInformation("Created processing job {JobId} for media {MediaId}", job.Id, job.MediaId);
            return Result<MediaProcessingJob>.Success(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating processing job for media {MediaId}", job.MediaId);
            return Result<MediaProcessingJob>.Failure($"Failed to create processing job: {ex.Message}");
        }
    }

    public async Task<Result<MediaProcessingJob>> GetProcessingJobByIdAsync(string jobId)
    {
        try
        {
            var job = await _jobsCollection
                .Find(j => j.Id == jobId)
                .FirstOrDefaultAsync();

            if (job == null)
            {
                return Result<MediaProcessingJob>.Failure("Processing job not found");
            }

            return Result<MediaProcessingJob>.Success(job);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting processing job {JobId}", jobId);
            return Result<MediaProcessingJob>.Failure($"Failed to get processing job: {ex.Message}");
        }
    }

    public async Task<Result<List<MediaProcessingJob>>> GetPendingJobsAsync(int limit = 10)
    {
        try
        {
            var jobs = await _jobsCollection
                .Find(j => j.Status == ProcessingStatus.Queued)
                .SortBy(j => j.CreatedAt)
                .Limit(limit)
                .ToListAsync();

            return Result<List<MediaProcessingJob>>.Success(jobs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending processing jobs");
            return Result<List<MediaProcessingJob>>.Failure($"Failed to get pending jobs: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateJobProgressAsync(string jobId, ProcessingStatus status, int progress, string? error = null)
    {
        try
        {
            var updateBuilder = Builders<MediaProcessingJob>.Update
                .Set(j => j.Status, status)
                .Set(j => j.Progress, progress)
                .Set(j => j.UpdatedAt, DateTime.UtcNow);

            if (status == ProcessingStatus.Processing && !await HasStartedAsync(jobId))
            {
                updateBuilder = updateBuilder.Set(j => j.StartedAt, DateTime.UtcNow);
            }

            if (!string.IsNullOrEmpty(error))
            {
                updateBuilder = updateBuilder.Set(j => j.Error, error);
            }

            var result = await _jobsCollection.UpdateOneAsync(j => j.Id == jobId, updateBuilder);

            if (result.MatchedCount == 0)
            {
                return Result<bool>.Failure("Processing job not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating job progress {JobId}", jobId);
            return Result<bool>.Failure($"Failed to update job progress: {ex.Message}");
        }
    }

    public async Task<Result<bool>> CompleteJobAsync(string jobId)
    {
        try
        {
            var update = Builders<MediaProcessingJob>.Update
                .Set(j => j.Status, ProcessingStatus.Completed)
                .Set(j => j.Progress, 100)
                .Set(j => j.CompletedAt, DateTime.UtcNow)
                .Set(j => j.UpdatedAt, DateTime.UtcNow);

            var result = await _jobsCollection.UpdateOneAsync(j => j.Id == jobId, update);

            if (result.MatchedCount == 0)
            {
                return Result<bool>.Failure("Processing job not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing job {JobId}", jobId);
            return Result<bool>.Failure($"Failed to complete job: {ex.Message}");
        }
    }

    public async Task<Result<bool>> FailJobAsync(string jobId, string error)
    {
        try
        {
            var job = await _jobsCollection.Find(j => j.Id == jobId).FirstOrDefaultAsync();
            if (job == null)
            {
                return Result<bool>.Failure("Processing job not found");
            }

            var updateBuilder = Builders<MediaProcessingJob>.Update
                .Set(j => j.Error, error)
                .Set(j => j.UpdatedAt, DateTime.UtcNow)
                .Inc(j => j.RetryCount, 1);

            // If max retries reached, mark as failed, otherwise requeue
            if (job.RetryCount + 1 >= job.MaxRetries)
            {
                updateBuilder = updateBuilder
                    .Set(j => j.Status, ProcessingStatus.Failed)
                    .Set(j => j.CompletedAt, DateTime.UtcNow);
            }
            else
            {
                updateBuilder = updateBuilder.Set(j => j.Status, ProcessingStatus.Queued);
            }

            var result = await _jobsCollection.UpdateOneAsync(j => j.Id == jobId, updateBuilder);

            if (result.MatchedCount == 0)
            {
                return Result<bool>.Failure("Processing job not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error failing job {JobId}", jobId);
            return Result<bool>.Failure($"Failed to fail job: {ex.Message}");
        }
    }

    // Search and Stats
    public async Task<Result<List<Media>>> SearchMediaAsync(string query, MediaType? type = null, int page = 1, int pageSize = 20)
    {
        try
        {
            var filterBuilder = Builders<Media>.Filter;
            var filter = filterBuilder.Ne(m => m.Status, MediaStatus.Deleted);

            // Search in filename, original filename, alt text, and tags
            var searchFilter = filterBuilder.Or(
                filterBuilder.Regex(m => m.Filename, new MongoDB.Bson.BsonRegularExpression(query, "i")),
                filterBuilder.Regex(m => m.OriginalFilename, new MongoDB.Bson.BsonRegularExpression(query, "i")),
                filterBuilder.Regex(m => m.AltText, new MongoDB.Bson.BsonRegularExpression(query, "i")),
                filterBuilder.AnyIn(m => m.Tags, new[] { query })
            );

            filter &= searchFilter;

            if (type.HasValue)
            {
                filter &= filterBuilder.Eq(m => m.MediaType, type.Value);
            }

            var media = await Collection
                .Find(filter)
                .SortByDescending(m => m.UploadedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Media>>.Success(media);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching media with query {Query}", query);
            return Result<List<Media>>.Failure($"Failed to search media: {ex.Message}");
        }
    }

    public async Task<Result<long>> GetUserStorageUsedAsync(Guid userId)
    {
        try
        {
            var filter = Builders<Media>.Filter.Eq(m => m.UserId, userId) &
                         Builders<Media>.Filter.Ne(m => m.Status, MediaStatus.Deleted);

            var pipeline = await Collection.Aggregate()
                .Match(filter)
                .Group(
                    m => m.UserId,
                    g => new { TotalSize = g.Sum(m => m.FileSize) })
                .FirstOrDefaultAsync();

            var totalSize = pipeline?.TotalSize ?? 0;
            return Result<long>.Success(totalSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting storage used for user {UserId}", userId);
            return Result<long>.Failure($"Failed to get storage used: {ex.Message}");
        }
    }

    public async Task<Result<int>> GetUserMediaCountAsync(Guid userId, MediaType? type = null)
    {
        try
        {
            var filterBuilder = Builders<Media>.Filter;
            var filter = filterBuilder.Eq(m => m.UserId, userId) &
                         filterBuilder.Ne(m => m.Status, MediaStatus.Deleted);

            if (type.HasValue)
            {
                filter &= filterBuilder.Eq(m => m.MediaType, type.Value);
            }

            var count = await Collection.CountDocumentsAsync(filter);
            return Result<int>.Success((int)count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media count for user {UserId}", userId);
            return Result<int>.Failure($"Failed to get media count: {ex.Message}");
        }
    }

    private async Task<bool> HasStartedAsync(string jobId)
    {
        var job = await _jobsCollection.Find(j => j.Id == jobId).FirstOrDefaultAsync();
        return job?.StartedAt != null;
    }
}
