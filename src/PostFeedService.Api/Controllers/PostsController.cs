using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using PostFeedService.Api.Models;
using PostFeedService.Api.Repositories;
using Shared.Contracts.Common;

namespace PostFeedService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PostsController : ControllerBase
{
    private readonly IPostRepository _repository;
    private readonly ILogger<PostsController> _logger;

    public PostsController(IPostRepository repository, ILogger<PostsController> logger)
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
    public async Task<ActionResult<ApiResponse<Post>>> CreatePost([FromBody] CreatePostRequest request)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        var post = new Post
        {
            UserId = userId,
            Username = username,
            Content = request.Content,
            ContentType = request.ContentType,
            MediaUrls = request.MediaUrls ?? new(),
            Poll = request.Poll,
            Location = request.Location,
            Mentions = request.Mentions ?? new(),
            Hashtags = request.Hashtags ?? new(),
            Visibility = request.Visibility,
            IsCommentsEnabled = request.IsCommentsEnabled ?? true
        };

        var result = await _repository.CreatePostAsync(post);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Post>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<Post>.SuccessResponse(result.Value));
    }

    [HttpGet("{postId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<Post>>> GetPost(string postId)
    {
        var result = await _repository.GetPostByIdAsync(postId);

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<Post>.ErrorResponse(result.Error));
        }

        // Increment view count
        await _repository.IncrementViewCountAsync(postId);

        return Ok(ApiResponse<Post>.SuccessResponse(result.Value));
    }

    [HttpPut("{postId}")]
    public async Task<ActionResult<ApiResponse<Post>>> UpdatePost(string postId, [FromBody] UpdatePostRequest request)
    {
        var userId = GetCurrentUserId();

        var postResult = await _repository.GetPostByIdAsync(postId);
        if (!postResult.IsSuccess)
        {
            return NotFound(ApiResponse<Post>.ErrorResponse(postResult.Error));
        }

        var post = postResult.Value;

        if (post.UserId != userId)
        {
            return Forbid();
        }

        // Update fields
        if (!string.IsNullOrWhiteSpace(request.Content))
            post.Content = request.Content;

        if (request.MediaUrls != null)
            post.MediaUrls = request.MediaUrls;

        if (request.Hashtags != null)
            post.Hashtags = request.Hashtags;

        if (request.Visibility.HasValue)
            post.Visibility = request.Visibility.Value;

        if (request.IsCommentsEnabled.HasValue)
            post.IsCommentsEnabled = request.IsCommentsEnabled.Value;

        var result = await _repository.UpdatePostAsync(postId, post);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Post>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<Post>.SuccessResponse(post));
    }

    [HttpDelete("{postId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePost(string postId)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.DeletePostAsync(postId, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpGet("user/{userId:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<Post>>>> GetUserPosts(
        Guid userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _repository.GetUserPostsAsync(userId, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Post>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Post>>.SuccessResponse(result.Value));
    }

    [HttpGet("feed/timeline")]
    public async Task<ActionResult<ApiResponse<List<Post>>>> GetTimelineFeed(
        [FromQuery] List<Guid>? followingIds,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.GetTimelineFeedAsync(userId, followingIds ?? new(), page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Post>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Post>>.SuccessResponse(result.Value));
    }

    [HttpGet("feed/explore")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<Post>>>> GetExploreFeed(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _repository.GetExploreFeedAsync(page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Post>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Post>>.SuccessResponse(result.Value));
    }

    [HttpGet("trending")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<Post>>>> GetTrendingPosts([FromQuery] int limit = 10)
    {
        var result = await _repository.GetTrendingPostsAsync(limit);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Post>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Post>>.SuccessResponse(result.Value));
    }

    [HttpGet("hashtag/{hashtag}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<Post>>>> GetPostsByHashtag(
        string hashtag,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _repository.GetPostsByHashtagAsync(hashtag, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Post>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Post>>.SuccessResponse(result.Value));
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<Post>>>> SearchPosts(
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(ApiResponse<List<Post>>.ErrorResponse("Search query is required"));
        }

        var result = await _repository.SearchPostsAsync(query, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Post>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Post>>.SuccessResponse(result.Value));
    }

    [HttpGet("hashtags/trending")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<Hashtag>>>> GetTrendingHashtags([FromQuery] int limit = 10)
    {
        var result = await _repository.GetTrendingHashtagsAsync(limit);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Hashtag>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Hashtag>>.SuccessResponse(result.Value));
    }
}

public record CreatePostRequest
{
    public required string Content { get; init; }
    public PostContentType ContentType { get; init; } = PostContentType.Text;
    public List<MediaItem>? MediaUrls { get; init; }
    public Poll? Poll { get; init; }
    public Location? Location { get; init; }
    public List<Guid>? Mentions { get; init; }
    public List<string>? Hashtags { get; init; }
    public PostVisibility Visibility { get; init; } = PostVisibility.Public;
    public bool? IsCommentsEnabled { get; init; }
}

public record UpdatePostRequest
{
    public string? Content { get; init; }
    public List<MediaItem>? MediaUrls { get; init; }
    public List<string>? Hashtags { get; init; }
    public PostVisibility? Visibility { get; init; }
    public bool? IsCommentsEnabled { get; init; }
}
