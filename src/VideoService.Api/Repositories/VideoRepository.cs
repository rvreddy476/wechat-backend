using MongoDB.Driver;
using VideoService.Api.Models;
using Shared.Domain.Common;

namespace VideoService.Api.Repositories;

public class VideoRepository : IVideoRepository
{
    private readonly IMongoCollection<Video> _videos;
    private readonly ILogger<VideoRepository> _logger;

    public VideoRepository(IMongoDatabase database, ILogger<VideoRepository> logger)
    {
        _videos = database.GetCollection<Video>("videos");
        _logger = logger;
    }

    public async Task<Result<Video>> CreateVideoAsync(Video video)
    {
        try
        {
            await _videos.InsertOneAsync(video);
            return Result<Video>.Success(video);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating video for user {UserId}", video.UserId);
            return Result.Failure<Video>("Failed to create video");
        }
    }

    public async Task<Result<Video>> GetVideoByIdAsync(string videoId)
    {
        try
        {
            var video = await _videos.Find(v => v.Id == videoId && !v.IsDeleted)
                .FirstOrDefaultAsync();

            if (video == null)
            {
                return Result.Failure<Video>("Video not found");
            }

            return Result<Video>.Success(video);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video {VideoId}", videoId);
            return Result.Failure<Video>("Failed to get video");
        }
    }

    public async Task<Result<bool>> UpdateVideoAsync(string videoId, Video video)
    {
        try
        {
            video.UpdatedAt = DateTime.UtcNow;

            var result = await _videos.ReplaceOneAsync(
                v => v.Id == videoId && !v.IsDeleted,
                video
            );

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Video not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating video {VideoId}", videoId);
            return Result.Failure<bool>("Failed to update video");
        }
    }

    public async Task<Result<bool>> DeleteVideoAsync(string videoId, Guid userId)
    {
        try
        {
            var update = Builders<Video>.Update
                .Set(v => v.IsDeleted, true)
                .Set(v => v.DeletedAt, DateTime.UtcNow)
                .Set(v => v.UpdatedAt, DateTime.UtcNow);

            var result = await _videos.UpdateOneAsync(
                v => v.Id == videoId && v.UserId == userId && !v.IsDeleted,
                update
            );

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Video not found or unauthorized");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting video {VideoId}", videoId);
            return Result.Failure<bool>("Failed to delete video");
        }
    }

    public async Task<Result<List<Video>>> GetUserVideosAsync(Guid userId, VideoType? type = null, int page = 1, int pageSize = 20)
    {
        try
        {
            var filterBuilder = Builders<Video>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(v => v.UserId, userId),
                filterBuilder.Eq(v => v.IsDeleted, false),
                filterBuilder.Eq(v => v.ProcessingStatus, ProcessingStatus.Ready)
            );

            if (type.HasValue)
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(v => v.VideoType, type.Value));
            }

            var videos = await _videos.Find(filter)
                .SortByDescending(v => v.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Video>>.Success(videos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting videos for user {UserId}", userId);
            return Result.Failure<List<Video>>("Failed to get user videos");
        }
    }

    public async Task<Result<bool>> UpdateProcessingStatusAsync(string videoId, ProcessingStatus status, int progress, string? error = null)
    {
        try
        {
            var update = Builders<Video>.Update
                .Set(v => v.ProcessingStatus, status)
                .Set(v => v.ProcessingProgress, progress)
                .Set(v => v.UpdatedAt, DateTime.UtcNow);

            if (error != null)
            {
                update = update.Set(v => v.ProcessingError, error);
            }

            if (status == ProcessingStatus.Ready)
            {
                update = update.Set(v => v.PublishedAt, DateTime.UtcNow);
            }

            var result = await _videos.UpdateOneAsync(
                v => v.Id == videoId,
                update
            );

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Video not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating processing status for video {VideoId}", videoId);
            return Result.Failure<bool>("Failed to update processing status");
        }
    }

    public async Task<Result<bool>> UpdateStreamingUrlAsync(string videoId, string streamingUrl, List<QualityVariant> qualityVariants)
    {
        try
        {
            var update = Builders<Video>.Update
                .Set(v => v.StreamingUrl, streamingUrl)
                .Set(v => v.QualityVariants, qualityVariants)
                .Set(v => v.UpdatedAt, DateTime.UtcNow);

            var result = await _videos.UpdateOneAsync(
                v => v.Id == videoId,
                update
            );

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Video not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating streaming URL for video {VideoId}", videoId);
            return Result.Failure<bool>("Failed to update streaming URL");
        }
    }

    public async Task<Result<bool>> UpdateThumbnailsAsync(string videoId, List<string> thumbnailUrls, int selectedIndex = 0)
    {
        try
        {
            var update = Builders<Video>.Update
                .Set(v => v.ThumbnailUrls, thumbnailUrls)
                .Set(v => v.SelectedThumbnailIndex, selectedIndex)
                .Set(v => v.UpdatedAt, DateTime.UtcNow);

            var result = await _videos.UpdateOneAsync(
                v => v.Id == videoId,
                update
            );

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Video not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating thumbnails for video {VideoId}", videoId);
            return Result.Failure<bool>("Failed to update thumbnails");
        }
    }

    public async Task<Result<List<Video>>> GetTrendingVideosAsync(VideoType? type = null, int limit = 20)
    {
        try
        {
            var yesterday = DateTime.UtcNow.AddDays(-1);

            var filterBuilder = Builders<Video>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(v => v.Visibility, VideoVisibility.Public),
                filterBuilder.Eq(v => v.IsDeleted, false),
                filterBuilder.Eq(v => v.ProcessingStatus, ProcessingStatus.Ready),
                filterBuilder.Gte(v => v.PublishedAt, yesterday)
            );

            if (type.HasValue)
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(v => v.VideoType, type.Value));
            }

            var videos = await _videos.Find(filter)
                .SortByDescending(v => v.Stats.ViewsCount)
                .ThenByDescending(v => v.Stats.LikesCount)
                .Limit(limit)
                .ToListAsync();

            return Result<List<Video>>.Success(videos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting trending videos");
            return Result.Failure<List<Video>>("Failed to get trending videos");
        }
    }

    public async Task<Result<List<Video>>> GetFeaturedVideosAsync(VideoType? type = null, int limit = 10)
    {
        try
        {
            var filterBuilder = Builders<Video>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(v => v.IsFeatured, true),
                filterBuilder.Eq(v => v.Visibility, VideoVisibility.Public),
                filterBuilder.Eq(v => v.IsDeleted, false),
                filterBuilder.Eq(v => v.ProcessingStatus, ProcessingStatus.Ready)
            );

            if (type.HasValue)
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(v => v.VideoType, type.Value));
            }

            var videos = await _videos.Find(filter)
                .SortByDescending(v => v.Stats.ViewsCount)
                .Limit(limit)
                .ToListAsync();

            return Result<List<Video>>.Success(videos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured videos");
            return Result.Failure<List<Video>>("Failed to get featured videos");
        }
    }

    public async Task<Result<List<Video>>> GetVideosByCategoryAsync(string category, int page = 1, int pageSize = 20)
    {
        try
        {
            var videos = await _videos.Find(v =>
                v.Category == category &&
                v.Visibility == VideoVisibility.Public &&
                !v.IsDeleted &&
                v.ProcessingStatus == ProcessingStatus.Ready
            )
                .SortByDescending(v => v.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Video>>.Success(videos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting videos by category {Category}", category);
            return Result.Failure<List<Video>>("Failed to get videos by category");
        }
    }

    public async Task<Result<List<Video>>> GetVideosByTagAsync(string tag, int page = 1, int pageSize = 20)
    {
        try
        {
            var videos = await _videos.Find(v =>
                v.Tags.Contains(tag) &&
                v.Visibility == VideoVisibility.Public &&
                !v.IsDeleted &&
                v.ProcessingStatus == ProcessingStatus.Ready
            )
                .SortByDescending(v => v.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Video>>.Success(videos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting videos by tag {Tag}", tag);
            return Result.Failure<List<Video>>("Failed to get videos by tag");
        }
    }

    public async Task<Result<List<Video>>> GetVideosByHashtagAsync(string hashtag, int page = 1, int pageSize = 20)
    {
        try
        {
            var normalizedHashtag = hashtag.ToLowerInvariant().TrimStart('#');

            var videos = await _videos.Find(v =>
                v.Hashtags.Any(h => h.ToLowerInvariant() == normalizedHashtag) &&
                v.Visibility == VideoVisibility.Public &&
                !v.IsDeleted &&
                v.ProcessingStatus == ProcessingStatus.Ready
            )
                .SortByDescending(v => v.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Video>>.Success(videos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting videos by hashtag {Hashtag}", hashtag);
            return Result.Failure<List<Video>>("Failed to get videos by hashtag");
        }
    }

    public async Task<Result<List<Video>>> GetRecommendedVideosAsync(Guid userId, VideoType? type = null, int limit = 20)
    {
        try
        {
            // Simple recommendation: most viewed public videos (can be enhanced with ML)
            var filterBuilder = Builders<Video>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(v => v.Visibility, VideoVisibility.Public),
                filterBuilder.Eq(v => v.IsDeleted, false),
                filterBuilder.Eq(v => v.ProcessingStatus, ProcessingStatus.Ready),
                filterBuilder.Ne(v => v.UserId, userId) // Exclude user's own videos
            );

            if (type.HasValue)
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(v => v.VideoType, type.Value));
            }

            var videos = await _videos.Find(filter)
                .SortByDescending(v => v.Stats.ViewsCount)
                .ThenByDescending(v => v.Stats.LikesCount)
                .Limit(limit)
                .ToListAsync();

            return Result<List<Video>>.Success(videos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recommended videos for user {UserId}", userId);
            return Result.Failure<List<Video>>("Failed to get recommended videos");
        }
    }

    public async Task<Result<List<Video>>> GetSubscriptionFeedAsync(List<Guid> subscribedUserIds, int page = 1, int pageSize = 20)
    {
        try
        {
            var videos = await _videos.Find(v =>
                subscribedUserIds.Contains(v.UserId) &&
                !v.IsDeleted &&
                v.ProcessingStatus == ProcessingStatus.Ready
            )
                .SortByDescending(v => v.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Video>>.Success(videos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscription feed");
            return Result.Failure<List<Video>>("Failed to get subscription feed");
        }
    }

    public async Task<Result<List<Video>>> GetExploreFeedAsync(VideoType? type = null, int page = 1, int pageSize = 20)
    {
        try
        {
            var filterBuilder = Builders<Video>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(v => v.Visibility, VideoVisibility.Public),
                filterBuilder.Eq(v => v.IsDeleted, false),
                filterBuilder.Eq(v => v.ProcessingStatus, ProcessingStatus.Ready)
            );

            if (type.HasValue)
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(v => v.VideoType, type.Value));
            }

            var videos = await _videos.Find(filter)
                .SortByDescending(v => v.Stats.ViewsCount)
                .ThenByDescending(v => v.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Video>>.Success(videos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting explore feed");
            return Result.Failure<List<Video>>("Failed to get explore feed");
        }
    }

    public async Task<Result<bool>> IncrementViewCountAsync(string videoId, Guid? userId = null)
    {
        try
        {
            var update = Builders<Video>.Update
                .Inc(v => v.Stats.ViewsCount, 1)
                .Set(v => v.UpdatedAt, DateTime.UtcNow);

            // If userId is provided, also increment unique views (simplified - in production use separate collection)
            if (userId.HasValue)
            {
                update = update.Inc(v => v.Stats.UniqueViewsCount, 1);
            }

            await _videos.UpdateOneAsync(v => v.Id == videoId, update);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing view count for video {VideoId}", videoId);
            return Result.Failure<bool>("Failed to increment view count");
        }
    }

    public async Task<Result<bool>> UpdateWatchTimeAsync(string videoId, int watchTimeSeconds, double watchPercentage)
    {
        try
        {
            var update = Builders<Video>.Update
                .Inc(v => v.Stats.WatchTimeSeconds, watchTimeSeconds)
                .Set(v => v.UpdatedAt, DateTime.UtcNow);

            // Update average watch percentage and completion rate (simplified calculation)
            var video = await _videos.Find(v => v.Id == videoId).FirstOrDefaultAsync();
            if (video != null)
            {
                var newAvgWatchPercentage = ((video.Stats.AverageWatchPercentage * video.Stats.ViewsCount) + watchPercentage) / (video.Stats.ViewsCount + 1);
                update = update.Set(v => v.Stats.AverageWatchPercentage, newAvgWatchPercentage);

                if (watchPercentage >= 90)
                {
                    var completions = (video.Stats.CompletionRate * video.Stats.ViewsCount) + 1;
                    var newCompletionRate = completions / (video.Stats.ViewsCount + 1);
                    update = update.Set(v => v.Stats.CompletionRate, newCompletionRate * 100);
                }
            }

            await _videos.UpdateOneAsync(v => v.Id == videoId, update);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating watch time for video {VideoId}", videoId);
            return Result.Failure<bool>("Failed to update watch time");
        }
    }

    public async Task<Result<bool>> UpdateStatsAsync(string videoId, string statField, int incrementBy)
    {
        try
        {
            var update = Builders<Video>.Update
                .Inc($"stats.{statField}", incrementBy)
                .Set(v => v.UpdatedAt, DateTime.UtcNow);

            var result = await _videos.UpdateOneAsync(
                v => v.Id == videoId && !v.IsDeleted,
                update
            );

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Video not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stats for video {VideoId}", videoId);
            return Result.Failure<bool>("Failed to update stats");
        }
    }

    public async Task<Result<List<Video>>> SearchVideosAsync(string searchTerm, VideoType? type = null, int page = 1, int pageSize = 20)
    {
        try
        {
            var filterBuilder = Builders<Video>.Filter;
            var filter = filterBuilder.And(
                filterBuilder.Eq(v => v.IsDeleted, false),
                filterBuilder.Eq(v => v.Visibility, VideoVisibility.Public),
                filterBuilder.Eq(v => v.ProcessingStatus, ProcessingStatus.Ready),
                filterBuilder.Or(
                    filterBuilder.Regex(v => v.Title, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i")),
                    filterBuilder.Regex(v => v.Description, new MongoDB.Bson.BsonRegularExpression(searchTerm, "i"))
                )
            );

            if (type.HasValue)
            {
                filter = filterBuilder.And(filter, filterBuilder.Eq(v => v.VideoType, type.Value));
            }

            var videos = await _videos.Find(filter)
                .SortByDescending(v => v.Stats.ViewsCount)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Video>>.Success(videos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching videos with term {SearchTerm}", searchTerm);
            return Result.Failure<List<Video>>("Failed to search videos");
        }
    }
}
