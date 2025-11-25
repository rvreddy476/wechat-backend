using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Realtime.Api.Services;
using System.Security.Claims;

namespace Realtime.Api.Hubs;

[Authorize]
public class PresenceHub : Hub
{
    private readonly IPresenceService _presenceService;
    private readonly ILogger<PresenceHub> _logger;

    public PresenceHub(IPresenceService presenceService, ILogger<PresenceHub> logger)
    {
        _presenceService = presenceService;
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
        var username = GetCurrentUsername();
        var connectionId = Context.ConnectionId;

        _logger.LogInformation("User {UserId} ({Username}) connected with connection {ConnectionId}",
            userId, username, connectionId);

        // Mark user as online
        await _presenceService.SetUserOnlineAsync(userId, connectionId);

        // Add to personal group
        await Groups.AddToGroupAsync(connectionId, $"user_{userId}");

        // Notify followers/friends about online status
        await Clients.Others.SendAsync("UserOnline", new
        {
            UserId = userId,
            Username = username,
            Timestamp = DateTime.UtcNow
        });

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();
        var connectionId = Context.ConnectionId;

        _logger.LogInformation("User {UserId} ({Username}) disconnected from connection {ConnectionId}",
            userId, username, connectionId);

        // Remove connection
        var isOffline = await _presenceService.RemoveConnectionAsync(userId, connectionId);

        // If user has no more connections, notify they're offline
        if (isOffline)
        {
            await Clients.Others.SendAsync("UserOffline", new
            {
                UserId = userId,
                Username = username,
                Timestamp = DateTime.UtcNow
            });
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Update user status (online, away, busy, etc.)
    public async Task UpdateStatus(string status)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        await _presenceService.UpdateUserStatusAsync(userId, status);

        _logger.LogInformation("User {UserId} updated status to {Status}", userId, status);

        // Broadcast status change
        await Clients.Others.SendAsync("UserStatusChanged", new
        {
            UserId = userId,
            Username = username,
            Status = status,
            Timestamp = DateTime.UtcNow
        });
    }

    // Join a specific group/room (e.g., for a specific conversation or live event)
    public async Task JoinGroup(string groupName)
    {
        var userId = GetCurrentUserId();

        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        _logger.LogInformation("User {UserId} joined group {GroupName}", userId, groupName);

        await Clients.Group(groupName).SendAsync("UserJoinedGroup", new
        {
            UserId = userId,
            GroupName = groupName,
            Timestamp = DateTime.UtcNow
        });
    }

    // Leave a specific group/room
    public async Task LeaveGroup(string groupName)
    {
        var userId = GetCurrentUserId();

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

        _logger.LogInformation("User {UserId} left group {GroupName}", userId, groupName);

        await Clients.Group(groupName).SendAsync("UserLeftGroup", new
        {
            UserId = userId,
            GroupName = groupName,
            Timestamp = DateTime.UtcNow
        });
    }

    // Send typing indicator
    public async Task SendTypingIndicator(string conversationId, bool isTyping)
    {
        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

        await Clients.Group($"conversation_{conversationId}").SendAsync("UserTyping", new
        {
            UserId = userId,
            Username = username,
            ConversationId = conversationId,
            IsTyping = isTyping,
            Timestamp = DateTime.UtcNow
        });
    }

    // Check if users are online
    public async Task<List<OnlineUserResponse>> CheckUsersOnline(List<Guid> userIds)
    {
        var onlineUsers = new List<OnlineUserResponse>();

        foreach (var userId in userIds)
        {
            var isOnline = await _presenceService.IsUserOnlineAsync(userId);
            var status = await _presenceService.GetUserStatusAsync(userId);
            var lastSeen = await _presenceService.GetLastSeenAsync(userId);

            onlineUsers.Add(new OnlineUserResponse
            {
                UserId = userId,
                IsOnline = isOnline,
                Status = status,
                LastSeen = lastSeen
            });
        }

        return onlineUsers;
    }

    // Heartbeat to keep connection alive and update last seen
    public async Task Heartbeat()
    {
        var userId = GetCurrentUserId();
        await _presenceService.UpdateHeartbeatAsync(userId, Context.ConnectionId);
    }

    // Broadcast event to specific users
    public async Task BroadcastToUsers(List<Guid> userIds, string eventName, object data)
    {
        foreach (var userId in userIds)
        {
            await Clients.User(userId.ToString()).SendAsync(eventName, data);
        }
    }

    // Ping for connection testing
    public Task<string> Ping()
    {
        return Task.FromResult("pong");
    }
}

public record OnlineUserResponse
{
    public required Guid UserId { get; init; }
    public required bool IsOnline { get; init; }
    public string? Status { get; init; }
    public DateTime? LastSeen { get; init; }
}
