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
public class MessagesController : ControllerBase
{
    private readonly IChatRepository _repository;
    private readonly ILogger<MessagesController> _logger;

    public MessagesController(IChatRepository repository, ILogger<MessagesController> logger)
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
    public async Task<ActionResult<ApiResponse<Message>>> SendMessage([FromBody] SendMessageRequest request)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        var message = new Message
        {
            ConversationId = request.ConversationId,
            SenderId = userId,
            SenderUsername = username,
            MessageType = request.MessageType,
            Content = request.Content,
            MediaUrl = request.MediaUrl,
            MediaThumbnailUrl = request.MediaThumbnailUrl,
            MediaDuration = request.MediaDuration,
            FileName = request.FileName,
            FileSize = request.FileSize,
            Location = request.Location,
            ReplyToMessageId = request.ReplyToMessageId,
            Mentions = request.Mentions ?? new()
        };

        var result = await _repository.SendMessageAsync(message);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Message>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<Message>.SuccessResponse(result.Value));
    }

    [HttpGet("{messageId}")]
    public async Task<ActionResult<ApiResponse<Message>>> GetMessage(string messageId)
    {
        var result = await _repository.GetMessageByIdAsync(messageId);

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<Message>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<Message>.SuccessResponse(result.Value));
    }

    [HttpPut("{messageId}")]
    public async Task<ActionResult<ApiResponse<Message>>> EditMessage(
        string messageId,
        [FromBody] EditMessageRequest request)
    {
        var userId = GetCurrentUserId();

        var messageResult = await _repository.GetMessageByIdAsync(messageId);
        if (!messageResult.IsSuccess)
        {
            return NotFound(ApiResponse<Message>.ErrorResponse(messageResult.Error));
        }

        var message = messageResult.Value;

        if (message.SenderId != userId)
        {
            return Forbid();
        }

        message.Content = request.Content;

        var result = await _repository.UpdateMessageAsync(messageId, message);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Message>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<Message>.SuccessResponse(message));
    }

    [HttpDelete("{messageId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMessage(
        string messageId,
        [FromQuery] bool deleteForEveryone = false)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.DeleteMessageAsync(messageId, userId, deleteForEveryone);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpGet("conversation/{conversationId}")]
    public async Task<ActionResult<ApiResponse<List<Message>>>> GetConversationMessages(
        string conversationId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _repository.GetConversationMessagesAsync(conversationId, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Message>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Message>>.SuccessResponse(result.Value));
    }

    [HttpGet("conversation/{conversationId}/before")]
    public async Task<ActionResult<ApiResponse<List<Message>>>> GetMessagesBefore(
        string conversationId,
        [FromQuery] DateTime before,
        [FromQuery] int limit = 50)
    {
        var result = await _repository.GetMessagesBeforeAsync(conversationId, before, limit);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Message>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Message>>.SuccessResponse(result.Value));
    }

    [HttpGet("conversation/{conversationId}/after")]
    public async Task<ActionResult<ApiResponse<List<Message>>>> GetMessagesAfter(
        string conversationId,
        [FromQuery] DateTime after,
        [FromQuery] int limit = 50)
    {
        var result = await _repository.GetMessagesAfterAsync(conversationId, after, limit);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Message>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Message>>.SuccessResponse(result.Value));
    }

    [HttpPost("{messageId}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(string messageId)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.MarkMessageAsReadAsync(messageId, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpGet("conversation/{conversationId}/search")]
    public async Task<ActionResult<ApiResponse<List<Message>>>> SearchMessages(
        string conversationId,
        [FromQuery] string query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(ApiResponse<List<Message>>.ErrorResponse("Search query is required"));
        }

        var result = await _repository.SearchMessagesAsync(conversationId, query, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Message>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Message>>.SuccessResponse(result.Value));
    }
}

public record SendMessageRequest
{
    public required string ConversationId { get; init; }
    public required MessageType MessageType { get; init; }
    public required string Content { get; init; }
    public string? MediaUrl { get; init; }
    public string? MediaThumbnailUrl { get; init; }
    public int? MediaDuration { get; init; }
    public string? FileName { get; init; }
    public long? FileSize { get; init; }
    public MessageLocation? Location { get; init; }
    public string? ReplyToMessageId { get; init; }
    public List<Guid>? Mentions { get; init; }
}

public record EditMessageRequest
{
    public required string Content { get; init; }
}
