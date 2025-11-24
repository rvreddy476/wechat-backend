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
public class ReactionsController : ControllerBase
{
    private readonly IPostRepository _repository;
    private readonly ILogger<ReactionsController> _logger;

    public ReactionsController(IPostRepository repository, ILogger<ReactionsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Reaction>>> AddReaction([FromBody] AddReactionRequest request)
    {
        var userId = GetCurrentUserId();

        var reaction = new Reaction
        {
            TargetId = request.TargetId,
            TargetType = request.TargetType,
            UserId = userId,
            ReactionType = request.ReactionType
        };

        var result = await _repository.AddReactionAsync(reaction);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Reaction>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<Reaction>.SuccessResponse(result.Value));
    }

    [HttpDelete("{targetType}/{targetId}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveReaction(string targetId, ReactionTargetType targetType)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.RemoveReactionAsync(targetId, userId, targetType);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpGet("{targetType}/{targetId}/user")]
    public async Task<ActionResult<ApiResponse<Reaction>>> GetUserReaction(string targetId, ReactionTargetType targetType)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.GetUserReactionAsync(targetId, userId, targetType);

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<Reaction>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<Reaction>.SuccessResponse(result.Value));
    }

    [HttpGet("{targetType}/{targetId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<Reaction>>>> GetReactions(
        string targetId,
        ReactionTargetType targetType,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _repository.GetReactionsAsync(targetId, targetType, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Reaction>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Reaction>>.SuccessResponse(result.Value));
    }

    [HttpGet("{targetType}/{targetId}/counts")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<Dictionary<ReactionType, int>>>> GetReactionCounts(
        string targetId,
        ReactionTargetType targetType)
    {
        var result = await _repository.GetReactionCountsAsync(targetId, targetType);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Dictionary<ReactionType, int>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<Dictionary<ReactionType, int>>.SuccessResponse(result.Value));
    }
}

public record AddReactionRequest
{
    public required string TargetId { get; init; }
    public required ReactionTargetType TargetType { get; init; }
    public required ReactionType ReactionType { get; init; }
}
