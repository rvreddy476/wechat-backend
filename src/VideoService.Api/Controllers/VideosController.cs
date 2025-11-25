using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VideoService.Api.Models;
using VideoService.Api.Repositories;
using Shared.Contracts.Common;

namespace VideoService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VideosController : ControllerBase
{
    private readonly IVideoRepository _repository;
    private readonly ILogger<VideosController> _logger;

    public VideosController(IVideoRepository repository, ILogger<VideosController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    private string GetCurrentUsername()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Video>>> UploadVideo([FromBody] UploadVideoRequest request)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        // Validate video type duration
        if (request.VideoType == VideoType.Short && request.Duration > 60)
        {
            return BadRequest(ApiResponse<Video>.ErrorResponse("Shorts must be 60 seconds or less"));
        }

        var video = new Video
        {
            UserId = userId,
            Username = username,
            Title = request.Title,
            Description = request.Description,
            VideoType = request.VideoType,
            Duration = request.Duration,
            OriginalFileName = request.OriginalFileName,
            FileSize = request.FileSize,
            Format = request.Format,
            Resolution = request.Resolution,
            ProcessingStatus = ProcessingStatus.Uploaded,
            SourceUrl = request.SourceUrl,
            Visibility = request.Visibility,
            Category = request.Category,
            Tags = request.Tags ?? new(),
            Hashtags = request.Hashtags ?? new(),
            Mentions = request.Mentions ?? new(),
            IsCommentsEnabled = request.IsCommentsEnabled ?? true,
            AgeRestricted = request.AgeRestricted ?? false,
            Metadata = request.Metadata ?? new()
        };

        var result = await _repository.CreateVideoAsync(video);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Video>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<Video>.SuccessResponse(result.Value));
    }

    [HttpGet("{videoId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<Video>>> GetVideo(string videoId)
    {
        var result = await _repository.GetVideoByIdAsync(videoId);

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<Video>.ErrorResponse(result.Error));
        }

        // Increment view count
        var userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : (Guid?)null;
        await _repository.IncrementViewCountAsync(videoId, userId);

        return Ok(ApiResponse<Video>.SuccessResponse(result.Value));
    }

    [HttpPut("{videoId}")]
    public async Task<ActionResult<ApiResponse<Video>>> UpdateVideo(string videoId, [FromBody] UpdateVideoRequest request)
    {
        var userId = GetCurrentUserId();

        var videoResult = await _repository.GetVideoByIdAsync(videoId);
        if (!videoResult.IsSuccess)
        {
            return NotFound(ApiResponse<Video>.ErrorResponse(videoResult.Error));
        }

        var video = videoResult.Value;

        if (video.UserId != userId)
        {
            return Forbid();
        }

        // Update fields
        if (!string.IsNullOrWhiteSpace(request.Title))
            video.Title = request.Title;

        if (request.Description != null)
            video.Description = request.Description;

        if (request.Visibility.HasValue)
            video.Visibility = request.Visibility.Value;

        if (request.Category != null)
            video.Category = request.Category;

        if (request.Tags != null)
            video.Tags = request.Tags;

        if (request.Hashtags != null)
            video.Hashtags = request.Hashtags;

        if (request.IsCommentsEnabled.HasValue)
            video.IsCommentsEnabled = request.IsCommentsEnabled.Value;

        if (request.SelectedThumbnailIndex.HasValue)
            video.SelectedThumbnailIndex = request.SelectedThumbnailIndex.Value;

        var updateResult = await _repository.UpdateVideoAsync(videoId, video);

        if (!updateResult.IsSuccess)
        {
            return BadRequest(ApiResponse<Video>.ErrorResponse(updateResult.Error));
        }

        return Ok(ApiResponse<Video>.SuccessResponse(video));
    }

    [HttpDelete("{videoId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteVideo(string videoId)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.DeleteVideoAsync(videoId, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpGet("user/{userId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<Video>>>> GetUserVideos(
        Guid userId,
        [FromQuery] VideoType? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _repository.GetUserVideosAsync(userId, type, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Video>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Video>>.SuccessResponse(result.Value));
    }

    [HttpGet("trending")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<Video>>>> GetTrendingVideos(
        [FromQuery] VideoType? type = null,
        [FromQuery] int limit = 20)
    {
        var result = await _repository.GetTrendingVideosAsync(type, limit);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Video>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Video>>.SuccessResponse(result.Value));
    }

    [HttpGet("featured")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<Video>>>> GetFeaturedVideos(
        [FromQuery] VideoType? type = null,
        [FromQuery] int limit = 10)
    {
        var result = await _repository.GetFeaturedVideosAsync(type, limit);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Video>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Video>>.SuccessResponse(result.Value));
    }

    [HttpGet("category/{category}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<Video>>>> GetVideosByCategory(
        string category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _repository.GetVideosByCategoryAsync(category, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Video>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Video>>.SuccessResponse(result.Value));
    }

    [HttpGet("tag/{tag}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<Video>>>> GetVideosByTag(
        string tag,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _repository.GetVideosByTagAsync(tag, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Video>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Video>>.SuccessResponse(result.Value));
    }

    [HttpGet("hashtag/{hashtag}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<Video>>>> GetVideosByHashtag(
        string hashtag,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _repository.GetVideosByHashtagAsync(hashtag, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Video>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Video>>.SuccessResponse(result.Value));
    }

    [HttpGet("recommended")]
    public async Task<ActionResult<ApiResponse<List<Video>>>> GetRecommendedVideos(
        [FromQuery] VideoType? type = null,
        [FromQuery] int limit = 20)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.GetRecommendedVideosAsync(userId, type, limit);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Video>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Video>>.SuccessResponse(result.Value));
    }

    [HttpGet("feed/subscriptions")]
    public async Task<ActionResult<ApiResponse<List<Video>>>> GetSubscriptionFeed(
        [FromQuery] List<Guid>? subscribedUserIds,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _repository.GetSubscriptionFeedAsync(subscribedUserIds ?? new(), page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Video>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Video>>.SuccessResponse(result.Value));
    }

    [HttpGet("feed/explore")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<Video>>>> GetExploreFeed(
        [FromQuery] VideoType? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _repository.GetExploreFeedAsync(type, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Video>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Video>>.SuccessResponse(result.Value));
    }

    [HttpPost("{videoId}/watch")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<bool>>> TrackWatchTime(
        string videoId,
        [FromBody] TrackWatchTimeRequest request)
    {
        var result = await _repository.UpdateWatchTimeAsync(videoId, request.WatchTimeSeconds, request.WatchPercentage);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<Video>>>> SearchVideos(
        [FromQuery] string query,
        [FromQuery] VideoType? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(ApiResponse<List<Video>>.ErrorResponse("Search query is required"));
        }

        var result = await _repository.SearchVideosAsync(query, type, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Video>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Video>>.SuccessResponse(result.Value));
    }

    // Processing endpoints (typically called by backend video processing service)
    [HttpPut("{videoId}/processing-status")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateProcessingStatus(
        string videoId,
        [FromBody] UpdateProcessingStatusRequest request)
    {
        var result = await _repository.UpdateProcessingStatusAsync(
            videoId,
            request.Status,
            request.Progress,
            request.Error
        );

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpPut("{videoId}/streaming-url")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStreamingUrl(
        string videoId,
        [FromBody] UpdateStreamingUrlRequest request)
    {
        var result = await _repository.UpdateStreamingUrlAsync(
            videoId,
            request.StreamingUrl,
            request.QualityVariants
        );

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpPut("{videoId}/thumbnails")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateThumbnails(
        string videoId,
        [FromBody] UpdateThumbnailsRequest request)
    {
        var result = await _repository.UpdateThumbnailsAsync(
            videoId,
            request.ThumbnailUrls,
            request.SelectedIndex
        );

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }
}

public record UploadVideoRequest
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public required VideoType VideoType { get; init; }
    public required int Duration { get; init; }
    public required string OriginalFileName { get; init; }
    public required long FileSize { get; init; }
    public required string Format { get; init; }
    public required VideoResolution Resolution { get; init; }
    public required string SourceUrl { get; init; }
    public VideoVisibility Visibility { get; init; } = VideoVisibility.Public;
    public string? Category { get; init; }
    public List<string>? Tags { get; init; }
    public List<string>? Hashtags { get; init; }
    public List<Guid>? Mentions { get; init; }
    public bool? IsCommentsEnabled { get; init; }
    public bool? AgeRestricted { get; init; }
    public VideoMetadata? Metadata { get; init; }
}

public record UpdateVideoRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public VideoVisibility? Visibility { get; init; }
    public string? Category { get; init; }
    public List<string>? Tags { get; init; }
    public List<string>? Hashtags { get; init; }
    public bool? IsCommentsEnabled { get; init; }
    public int? SelectedThumbnailIndex { get; init; }
}

public record TrackWatchTimeRequest
{
    public required int WatchTimeSeconds { get; init; }
    public required double WatchPercentage { get; init; }
}

public record UpdateProcessingStatusRequest
{
    public required ProcessingStatus Status { get; init; }
    public required int Progress { get; init; }
    public string? Error { get; init; }
}

public record UpdateStreamingUrlRequest
{
    public required string StreamingUrl { get; init; }
    public required List<QualityVariant> QualityVariants { get; init; }
}

public record UpdateThumbnailsRequest
{
    public required List<string> ThumbnailUrls { get; init; }
    public int SelectedIndex { get; init; } = 0;
}
