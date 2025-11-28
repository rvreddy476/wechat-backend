using ChatService.Application.Messages.Commands.SendMessage;
using ChatService.Application.Messages.Queries.GetMessages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Contracts.Common;
using System.Security.Claims;

namespace ChatService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return Guid.Parse(userIdClaim?.Value ?? throw new UnauthorizedAccessException());
    }

    private string GetCurrentUsername()
    {
        return User.FindFirst("username") ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        var command = new SendMessageCommand
        {
            ConversationId = request.ConversationId,
            SenderId = userId,
            SenderUsername = username,
            Content = request.Content,
            MessageType = request.MessageType,
            MediaUrl = request.MediaUrl,
            ReplyToMessageId = request.ReplyToMessageId
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            result.Value.Id,
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
        var query = new GetMessagesQuery
        {
            ConversationId = conversationId,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<object>.SuccessResponse(result.Value));
    }
}

public record SendMessageRequest
{
    public required string ConversationId { get; init; }
    public required string Content { get; init; }
    public ChatService.Domain.Entities.MessageType MessageType { get; init; }
    public string? MediaUrl { get; init; }
    public string? ReplyToMessageId { get; init; }
}
