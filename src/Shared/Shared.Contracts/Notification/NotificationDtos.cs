namespace Shared.Contracts.Notification;

/// <summary>
/// Notification DTO
/// </summary>
public record NotificationDto
{
    public string Id { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public NotificationType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Dictionary<string, string> Data { get; init; } = new();
    public bool IsRead { get; init; }
    public DateTime? ReadAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? ActionUrl { get; init; }
    public string? ImageUrl { get; init; }
}

/// <summary>
/// Send notification request
/// </summary>
public record SendNotificationRequest
{
    public string UserId { get; init; } = string.Empty;
    public NotificationType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Dictionary<string, string> Data { get; init; } = new();
    public string? ActionUrl { get; init; }
    public string? ImageUrl { get; init; }
}

/// <summary>
/// Send bulk notification request
/// </summary>
public record SendBulkNotificationRequest
{
    public List<string> UserIds { get; init; } = new();
    public NotificationType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public Dictionary<string, string> Data { get; init; } = new();
    public string? ActionUrl { get; init; }
}

/// <summary>
/// Mark notifications as read request
/// </summary>
public record MarkNotificationsAsReadRequest
{
    public List<string> NotificationIds { get; init; } = new();
}

/// <summary>
/// Push notification token registration
/// </summary>
public record RegisterPushTokenRequest
{
    public string DeviceToken { get; init; } = string.Empty;
    public DevicePlatform Platform { get; init; }
    public string DeviceId { get; init; } = string.Empty;
}

/// <summary>
/// Notification type enumeration
/// </summary>
public enum NotificationType
{
    FriendRequest,
    FriendRequestAccepted,
    NewMessage,
    PostLike,
    PostComment,
    PostMention,
    NewFollower,
    System
}

/// <summary>
/// Device platform enumeration
/// </summary>
public enum DevicePlatform
{
    iOS,
    Android,
    Web
}
