using Chat.Api.Hubs;
using Chat.Application.Messages.Commands.SendMessage;
using Chat.Application.Messages.Queries.GetMessages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Shared.Contracts.Common;
using System.Security.Claims;

namespace Chat.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<ChatHub> _hubContext;

    public MessagesController(IMediator mediator, IHubContext<ChatHub> hubContext)
    {
        _mediator = mediator;
        _hubContext = hubContext;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = GetUserId();
        var username = GetUsername();

        var command = new SendMessageCommand(
            request.ConversationId,
            userId,
            username,
            request.Content,
            request.MessageType,
            request.MediaUrl,
            request.ReplyToMessageId
        );

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));

        // Send via SignalR to all users in the conversation
        await _hubContext.Clients.Group(request.ConversationId).SendAsync("ReceiveMessage", new
        {
            conversationId = request.ConversationId,
            messageId = result.Value.Id,
            senderId = userId,
            senderUsername = username,
            content = request.Content,
            messageType = request.MessageType,
            createdAt = result.Value.CreatedAt
        });

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            result.Value.Id,
            result.Value.ConversationId,
            result.Value.Content,
            result.Value.CreatedAt
        }));
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages(
        [FromQuery] string conversationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new GetMessagesQuery(conversationId, page, pageSize);
        var result = await _mediator.Send(query);

        return result.IsSuccess
            ? Ok(ApiResponse<object>.SuccessResponse(result.Value))
            : BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
    }

    [HttpPost("{messageId}/read")]
    public async Task<IActionResult> MarkAsRead(string messageId, [FromQuery] string conversationId)
    {
        // Implementation for marking message as read
        await _hubContext.Clients.Group(conversationId).SendAsync("MessageRead", new
        {
            conversationId,
            messageId,
            userId = GetUserId(),
            readAt = DateTime.UtcNow
        });

        return Ok(ApiResponse<object>.SuccessResponse(true));
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

public record SendMessageRequest
{
    public required string ConversationId { get; init; }
    public required string Content { get; init; }
    public Chat.Domain.Entities.MessageType MessageType { get; init; } = Chat.Domain.Entities.MessageType.Text;
    public string? MediaUrl { get; init; }
    public string? ReplyToMessageId { get; init; }
}
