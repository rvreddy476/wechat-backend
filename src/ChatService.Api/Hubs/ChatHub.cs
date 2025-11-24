using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using ChatService.Api.Models;
using ChatService.Api.Repositories;

namespace ChatService.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatRepository _repository;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IChatRepository repository, ILogger<ChatHub> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    private string GetCurrentUsername()
    {
        return Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} connected to ChatHub with connection {ConnectionId}", userId, Context.ConnectionId);

        // Join user to their personal channel
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

        // Get user's conversations and join those groups
        var conversationsResult = await _repository.GetUserConversationsAsync(userId, 1, 100);
        if (conversationsResult.IsSuccess)
        {
            foreach (var conversation in conversationsResult.Value)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversation.Id}");
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} disconnected from ChatHub", userId);

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        _logger.LogInformation("User {UserId} joined conversation {ConversationId}", GetCurrentUserId(), conversationId);
    }

    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
        _logger.LogInformation("User {UserId} left conversation {ConversationId}", GetCurrentUserId(), conversationId);
    }

    public async Task SendMessage(string conversationId, string content, MessageType messageType, string? mediaUrl = null)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        var message = new Message
        {
            ConversationId = conversationId,
            SenderId = userId,
            SenderUsername = username,
            MessageType = messageType,
            Content = content,
            MediaUrl = mediaUrl
        };

        var result = await _repository.SendMessageAsync(message);

        if (result.IsSuccess)
        {
            // Broadcast message to all participants in the conversation
            await Clients.Group($"conversation_{conversationId}")
                .SendAsync("ReceiveMessage", message);

            _logger.LogInformation("Message {MessageId} sent to conversation {ConversationId}", message.Id, conversationId);
        }
        else
        {
            await Clients.Caller.SendAsync("Error", result.Error);
        }
    }

    public async Task SendTypingIndicator(string conversationId, bool isTyping)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        // Broadcast typing indicator to all participants except sender
        await Clients.OthersInGroup($"conversation_{conversationId}")
            .SendAsync("UserTyping", new
            {
                userId,
                username,
                conversationId,
                isTyping,
                timestamp = DateTime.UtcNow
            });
    }

    public async Task MarkMessageAsRead(string messageId, string conversationId)
    {
        var userId = GetCurrentUserId();

        var result = await _repository.MarkMessageAsReadAsync(messageId, userId);

        if (result.IsSuccess)
        {
            // Notify other participants that message was read
            await Clients.OthersInGroup($"conversation_{conversationId}")
                .SendAsync("MessageRead", new
                {
                    messageId,
                    userId,
                    readAt = DateTime.UtcNow
                });
        }
    }

    public async Task MarkConversationAsRead(string conversationId)
    {
        var userId = GetCurrentUserId();

        var result = await _repository.MarkConversationAsReadAsync(conversationId, userId);

        if (result.IsSuccess)
        {
            await Clients.Group($"conversation_{conversationId}")
                .SendAsync("ConversationRead", new
                {
                    conversationId,
                    userId,
                    readAt = DateTime.UtcNow
                });
        }
    }

    public async Task EditMessage(string messageId, string conversationId, string newContent)
    {
        var userId = GetCurrentUserId();

        var messageResult = await _repository.GetMessageByIdAsync(messageId);
        if (!messageResult.IsSuccess || messageResult.Value.SenderId != userId)
        {
            await Clients.Caller.SendAsync("Error", "Cannot edit message");
            return;
        }

        var message = messageResult.Value;
        message.Content = newContent;

        var result = await _repository.UpdateMessageAsync(messageId, message);

        if (result.IsSuccess)
        {
            await Clients.Group($"conversation_{conversationId}")
                .SendAsync("MessageEdited", message);
        }
    }

    public async Task DeleteMessage(string messageId, string conversationId, bool deleteForEveryone)
    {
        var userId = GetCurrentUserId();

        var result = await _repository.DeleteMessageAsync(messageId, userId, deleteForEveryone);

        if (result.IsSuccess)
        {
            if (deleteForEveryone)
            {
                await Clients.Group($"conversation_{conversationId}")
                    .SendAsync("MessageDeleted", new { messageId, deletedForEveryone = true });
            }
            else
            {
                await Clients.Caller.SendAsync("MessageDeleted", new { messageId, deletedForEveryone = false });
            }
        }
    }
}
