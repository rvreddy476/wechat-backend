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
public class CommentsController : ControllerBase
{
    private readonly IPostRepository _repository;
    private readonly ILogger<CommentsController> _logger;

    public CommentsController(IPostRepository repository, ILogger<CommentsController> logger)
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
    public async Task<ActionResult<ApiResponse<Comment>>> CreateComment([FromBody] CreateCommentRequest request)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        // Validate parent comment if it's a reply
        int level = 0;
        if (!string.IsNullOrEmpty(request.ParentCommentId))
        {
            var parentResult = await _repository.GetCommentByIdAsync(request.ParentCommentId);
            if (!parentResult.IsSuccess)
            {
                return BadRequest(ApiResponse<Comment>.ErrorResponse("Parent comment not found"));
            }

            level = parentResult.Value.Level + 1;

            if (level > 5)
            {
                return BadRequest(ApiResponse<Comment>.ErrorResponse("Maximum reply depth exceeded (5 levels)"));
            }
        }

        var comment = new Comment
        {
            PostId = request.PostId,
            UserId = userId,
            Username = username,
            Content = request.Content,
            ParentCommentId = request.ParentCommentId,
            Level = level,
            Mentions = request.Mentions ?? new(),
            MediaUrl = request.MediaUrl
        };

        var result = await _repository.CreateCommentAsync(comment);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Comment>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<Comment>.SuccessResponse(result.Value));
    }

    [HttpGet("{commentId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<Comment>>> GetComment(string commentId)
    {
        var result = await _repository.GetCommentByIdAsync(commentId);

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<Comment>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<Comment>.SuccessResponse(result.Value));
    }

    [HttpPut("{commentId}")]
    public async Task<ActionResult<ApiResponse<Comment>>> UpdateComment(string commentId, [FromBody] UpdateCommentRequest request)
    {
        var userId = GetCurrentUserId();

        var commentResult = await _repository.GetCommentByIdAsync(commentId);
        if (!commentResult.IsSuccess)
        {
            return NotFound(ApiResponse<Comment>.ErrorResponse(commentResult.Error));
        }

        var comment = commentResult.Value;

        if (comment.UserId != userId)
        {
            return Forbid();
        }

        if (!string.IsNullOrWhiteSpace(request.Content))
        {
            comment.Content = request.Content;
        }

        if (request.MediaUrl != null)
        {
            comment.MediaUrl = request.MediaUrl;
        }

        var result = await _repository.UpdateCommentAsync(commentId, comment);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Comment>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<Comment>.SuccessResponse(comment));
    }

    [HttpDelete("{commentId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteComment(string commentId)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.DeleteCommentAsync(commentId, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpGet("post/{postId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<Comment>>>> GetPostComments(
        string postId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _repository.GetPostCommentsAsync(postId, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Comment>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Comment>>.SuccessResponse(result.Value));
    }

    [HttpGet("{commentId}/replies")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<Comment>>>> GetCommentReplies(
        string commentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _repository.GetCommentRepliesAsync(commentId, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Comment>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Comment>>.SuccessResponse(result.Value));
    }
}

public record CreateCommentRequest
{
    public required string PostId { get; init; }
    public required string Content { get; init; }
    public string? ParentCommentId { get; init; }
    public List<Guid>? Mentions { get; init; }
    public string? MediaUrl { get; init; }
}

public record UpdateCommentRequest
{
    public string? Content { get; init; }
    public string? MediaUrl { get; init; }
}
