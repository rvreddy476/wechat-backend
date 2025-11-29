using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace Chat.Api.Hubs;

[Authorize]
public class ChatHub : Hub
{
    // Track online users
    private static readonly ConcurrentDictionary<string, UserConnection> OnlineUsers = new();
    
    // Track typing indicators
    private static readonly ConcurrentDictionary<string, HashSet<string>> TypingUsers = new();

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var username = GetUsername();

        OnlineUsers[userId] = new UserConnection
        {
            UserId = userId,
            Username = username,
            ConnectionId = Context.ConnectionId,
            ConnectedAt = DateTime.UtcNow
        };

        // Notify all clients that user is online
        await Clients.All.SendAsync("UserOnline", new { userId, username });

        // Send list of online users to the connected user
        var onlineUsersList = OnlineUsers.Values.Select(u => new { u.UserId, u.Username }).ToList();
        await Clients.Caller.SendAsync("OnlineUsers", onlineUsersList);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        
        if (OnlineUsers.TryRemove(userId, out var user))
        {
            await Clients.All.SendAsync("UserOffline", new { userId, username = user.Username });
        }

        // Remove from all typing indicators
        foreach (var conversationId in TypingUsers.Keys)
        {
            if (TypingUsers.TryGetValue(conversationId, out var typingSet))
            {
                if (typingSet.Remove(userId))
                {
                    await Clients.Group(conversationId).SendAsync("UserStoppedTyping", conversationId, userId);
                }
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinConversation(string conversationId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        await Clients.Group(conversationId).SendAsync("UserJoinedConversation", conversationId, GetUserId());
    }

    public async Task LeaveConversation(string conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
        await Clients.Group(conversationId).SendAsync("UserLeftConversation", conversationId, GetUserId());
    }

    public async Task SendMessage(string conversationId, string messageId, string content, string senderUsername)
    {
        await Clients.Group(conversationId).SendAsync("ReceiveMessage", new
        {
            conversationId,
            messageId,
            senderId = GetUserId(),
            senderUsername,
            content,
            createdAt = DateTime.UtcNow
        });
    }

    public async Task TypingIndicator(string conversationId, bool isTyping)
    {
        var userId = GetUserId();

        if (!TypingUsers.ContainsKey(conversationId))
        {
            TypingUsers[conversationId] = new HashSet<string>();
        }

        if (isTyping)
        {
            TypingUsers[conversationId].Add(userId);
            await Clients.GroupExcept(conversationId, Context.ConnectionId)
                .SendAsync("UserTyping", conversationId, userId, GetUsername());
        }
        else
        {
            TypingUsers[conversationId].Remove(userId);
            await Clients.GroupExcept(conversationId, Context.ConnectionId)
                .SendAsync("UserStoppedTyping", conversationId, userId);
        }
    }

    public async Task MessageRead(string conversationId, string messageId)
    {
        await Clients.Group(conversationId).SendAsync("MessageRead", new
        {
            conversationId,
            messageId,
            userId = GetUserId(),
            readAt = DateTime.UtcNow
        });
    }

    public async Task MessageDeleted(string conversationId, string messageId)
    {
        await Clients.Group(conversationId).SendAsync("MessageDeleted", conversationId, messageId);
    }

    private string GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier) ?? Context.User?.FindFirst("userId");
        return userIdClaim?.Value ?? throw new UnauthorizedAccessException("User ID not found");
    }

    private string GetUsername()
    {
        var usernameClaim = Context.User?.FindFirst("username") ?? Context.User?.FindFirst(ClaimTypes.Name);
        return usernameClaim?.Value ?? "Unknown";
    }
}

public class UserConnection
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
}
