using VideoService.Api.Models;
using Shared.Domain.Common;

namespace VideoService.Api.Repositories;

public interface IVideoRepository
{
    // Video Management
    Task<Result<Video>> CreateVideoAsync(Video video);
    Task<Result<Video>> GetVideoByIdAsync(string videoId);
    Task<Result<bool>> UpdateVideoAsync(string videoId, Video video);
    Task<Result<bool>> DeleteVideoAsync(string videoId, Guid userId);
    Task<Result<List<Video>>> GetUserVideosAsync(Guid userId, VideoType? type = null, int page = 1, int pageSize = 20);

    // Processing
    Task<Result<bool>> UpdateProcessingStatusAsync(string videoId, ProcessingStatus status, int progress, string? error = null);
    Task<Result<bool>> UpdateStreamingUrlAsync(string videoId, string streamingUrl, List<QualityVariant> qualityVariants);
    Task<Result<bool>> UpdateThumbnailsAsync(string videoId, List<string> thumbnailUrls, int selectedIndex = 0);

    // Discovery
    Task<Result<List<Video>>> GetTrendingVideosAsync(VideoType? type = null, int limit = 20);
    Task<Result<List<Video>>> GetFeaturedVideosAsync(VideoType? type = null, int limit = 10);
    Task<Result<List<Video>>> GetVideosByCategoryAsync(string category, int page = 1, int pageSize = 20);
    Task<Result<List<Video>>> GetVideosByTagAsync(string tag, int page = 1, int pageSize = 20);
    Task<Result<List<Video>>> GetVideosByHashtagAsync(string hashtag, int page = 1, int pageSize = 20);
    Task<Result<List<Video>>> GetRecommendedVideosAsync(Guid userId, VideoType? type = null, int limit = 20);

    // Feed
    Task<Result<List<Video>>> GetSubscriptionFeedAsync(List<Guid> subscribedUserIds, int page = 1, int pageSize = 20);
    Task<Result<List<Video>>> GetExploreFeedAsync(VideoType? type = null, int page = 1, int pageSize = 20);

    // Stats
    Task<Result<bool>> IncrementViewCountAsync(string videoId, Guid? userId = null);
    Task<Result<bool>> UpdateWatchTimeAsync(string videoId, int watchTimeSeconds, double watchPercentage);
    Task<Result<bool>> UpdateStatsAsync(string videoId, string statField, int incrementBy);

    // Search
    Task<Result<List<Video>>> SearchVideosAsync(string searchTerm, VideoType? type = null, int page = 1, int pageSize = 20);
}
