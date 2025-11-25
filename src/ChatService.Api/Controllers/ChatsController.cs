using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ChatService.Api.Models;
using ChatService.Api.Repositories;
using Shared.Contracts.Common;

namespace ChatService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatsController : ControllerBase
{
    private readonly IChatRepository _repository;
    private readonly ILogger<ChatsController> _logger;

    public ChatsController(IChatRepository repository, ILogger<ChatsController> logger)
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
    public async Task<ActionResult<ApiResponse<Conversation>>> CreateConversation([FromBody] CreateConversationRequest request)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        // Check if one-to-one conversation already exists
        if (request.Type == ConversationType.OneToOne && request.ParticipantIds.Count == 1)
        {
            var existingResult = await _repository.GetOneToOneConversationAsync(userId, request.ParticipantIds[0]);
            if (existingResult.IsSuccess)
            {
                return Ok(ApiResponse<Conversation>.SuccessResponse(existingResult.Value));
            }
        }

        var participants = new List<Participant>
        {
            new Participant
            {
                UserId = userId,
                Username = username,
                JoinedAt = DateTime.UtcNow
            }
        };

        // Add other participants
        foreach (var participantId in request.ParticipantIds)
        {
            participants.Add(new Participant
            {
                UserId = participantId,
                Username = request.ParticipantUsernames?.FirstOrDefault() ?? "User",
                JoinedAt = DateTime.UtcNow
            });
        }

        var conversation = new Conversation
        {
            Type = request.Type,
            Participants = participants,
            GroupName = request.GroupName,
            GroupAvatarUrl = request.GroupAvatarUrl,
            GroupDescription = request.GroupDescription,
            CreatedBy = userId,
            Admins = new List<Guid> { userId }
        };

        var result = await _repository.CreateConversationAsync(conversation);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Conversation>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<Conversation>.SuccessResponse(result.Value));
    }

    [HttpGet("{conversationId}")]
    public async Task<ActionResult<ApiResponse<Conversation>>> GetConversation(string conversationId)
    {
        var result = await _repository.GetConversationByIdAsync(conversationId);

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<Conversation>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<Conversation>.SuccessResponse(result.Value));
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Conversation>>>> GetUserConversations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.GetUserConversationsAsync(userId, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Conversation>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Conversation>>.SuccessResponse(result.Value));
    }

    [HttpPut("{conversationId}")]
    public async Task<ActionResult<ApiResponse<Conversation>>> UpdateConversation(
        string conversationId,
        [FromBody] UpdateConversationRequest request)
    {
        var userId = GetCurrentUserId();

        var conversationResult = await _repository.GetConversationByIdAsync(conversationId);
        if (!conversationResult.IsSuccess)
        {
            return NotFound(ApiResponse<Conversation>.ErrorResponse(conversationResult.Error));
        }

        var conversation = conversationResult.Value;

        // Check if user is admin for group conversations
        if (conversation.Type == ConversationType.Group && !conversation.Admins.Contains(userId))
        {
            return Forbid();
        }

        // Update fields
        if (!string.IsNullOrWhiteSpace(request.GroupName))
            conversation.GroupName = request.GroupName;

        if (request.GroupAvatarUrl != null)
            conversation.GroupAvatarUrl = request.GroupAvatarUrl;

        if (request.GroupDescription != null)
            conversation.GroupDescription = request.GroupDescription;

        var result = await _repository.UpdateConversationAsync(conversationId, conversation);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Conversation>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<Conversation>.SuccessResponse(conversation));
    }

    [HttpDelete("{conversationId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteConversation(string conversationId)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.DeleteConversationAsync(conversationId, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpPost("{conversationId}/participants")]
    public async Task<ActionResult<ApiResponse<bool>>> AddParticipant(
        string conversationId,
        [FromBody] AddParticipantRequest request)
    {
        var userId = GetCurrentUserId();

        var conversationResult = await _repository.GetConversationByIdAsync(conversationId);
        if (!conversationResult.IsSuccess)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(conversationResult.Error));
        }

        var conversation = conversationResult.Value;

        if (!conversation.Admins.Contains(userId))
        {
            return Forbid();
        }

        var participant = new Participant
        {
            UserId = request.UserId,
            Username = request.Username,
            JoinedAt = DateTime.UtcNow
        };

        var result = await _repository.AddParticipantAsync(conversationId, participant);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpDelete("{conversationId}/participants/{participantUserId:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveParticipant(
        string conversationId,
        Guid participantUserId)
    {
        var userId = GetCurrentUserId();

        var conversationResult = await _repository.GetConversationByIdAsync(conversationId);
        if (!conversationResult.IsSuccess)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(conversationResult.Error));
        }

        var conversation = conversationResult.Value;

        // User can remove themselves or admin can remove others
        if (participantUserId != userId && !conversation.Admins.Contains(userId))
        {
            return Forbid();
        }

        var result = await _repository.RemoveParticipantAsync(conversationId, participantUserId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpPost("{conversationId}/mute")]
    public async Task<ActionResult<ApiResponse<bool>>> MuteConversation(
        string conversationId,
        [FromBody] MuteConversationRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.MuteConversationAsync(conversationId, userId, request.MutedUntil);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpPost("{conversationId}/unmute")]
    public async Task<ActionResult<ApiResponse<bool>>> UnmuteConversation(string conversationId)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.MuteConversationAsync(conversationId, userId, null);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpGet("{conversationId}/unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount(string conversationId)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.GetUnreadMessageCountAsync(conversationId, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<int>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<int>.SuccessResponse(result.Value));
    }
}

public record CreateConversationRequest
{
    public required ConversationType Type { get; init; }
    public required List<Guid> ParticipantIds { get; init; }
    public List<string>? ParticipantUsernames { get; init; }
    public string? GroupName { get; init; }
    public string? GroupAvatarUrl { get; init; }
    public string? GroupDescription { get; init; }
}

public record UpdateConversationRequest
{
    public string? GroupName { get; init; }
    public string? GroupAvatarUrl { get; init; }
    public string? GroupDescription { get; init; }
}

public record AddParticipantRequest
{
    public required Guid UserId { get; init; }
    public required string Username { get; init; }
}

public record MuteConversationRequest
{
    public DateTime? MutedUntil { get; init; }
}
