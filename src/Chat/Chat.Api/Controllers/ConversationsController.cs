using Chat.Application.Conversations.Commands.CreateConversation;
using Chat.Application.Conversations.Queries.GetConversations;
using Chat.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Common;
using System.Security.Claims;

namespace Chat.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConversationsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
    {
        var userId = GetUserId();
        var username = GetUsername();

        var participantUsernames = new Dictionary<Guid, string> { { userId, username } };
        foreach (var participant in request.Participants)
        {
            participantUsernames[participant.UserId] = participant.Username;
        }

        var command = new CreateConversationCommand(
            request.Type,
            userId,
            request.Participants.Select(p => p.UserId).ToList(),
            participantUsernames,
            request.GroupName,
            request.GroupAvatarUrl
        );

        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(ApiResponse<object>.SuccessResponse(new
            {
                result.Value.Id,
                result.Value.Type,
                result.Value.Participants,
                result.Value.GroupName,
                result.Value.CreatedAt
            }))
            : BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
    }

    [HttpGet]
    public async Task<IActionResult> GetConversations([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var query = new GetConversationsQuery(userId, page, pageSize);
        var result = await _mediator.Send(query);

        return result.IsSuccess
            ? Ok(ApiResponse<object>.SuccessResponse(result.Value))
            : BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("userId");
        return Guid.Parse(userIdClaim?.Value ?? throw new UnauthorizedAccessException());
    }

    private string GetUsername()
    {
        var usernameClaim = User.FindFirst("username") ?? User.FindFirst(ClaimTypes.Name);
        return usernameClaim?.Value ?? "Unknown";
    }
}

public record CreateConversationRequest
{
    public ConversationType Type { get; init; }
    public List<ParticipantRequest> Participants { get; init; } = new();
    public string? GroupName { get; init; }
    public string? GroupAvatarUrl { get; init; }
}

public record ParticipantRequest
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
}
