using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Domain.Common;

namespace NotificationService.Api.Models;

public class Notification : Entity<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public override string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("recipientId")]
    [BsonRepresentation(BsonType.String)]
    public Guid RecipientId { get; set; }

    [BsonElement("senderId")]
    [BsonRepresentation(BsonType.String)]
    public Guid? SenderId { get; set; }

    [BsonElement("senderUsername")]
    public string? SenderUsername { get; set; }

    [BsonElement("senderAvatarUrl")]
    public string? SenderAvatarUrl { get; set; }

    [BsonElement("type")]
    public NotificationType Type { get; set; }

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("message")]
    public string Message { get; set; } = string.Empty;

    [BsonElement("entityId")]
    public string? EntityId { get; set; } // ID of related entity (post, comment, video, etc.)

    [BsonElement("entityType")]
    public EntityType? EntityType { get; set; }

    [BsonElement("actionUrl")]
    public string? ActionUrl { get; set; } // Deep link or web URL

    [BsonElement("imageUrl")]
    public string? ImageUrl { get; set; }

    [BsonElement("data")]
    public Dictionary<string, string>? Data { get; set; } // Additional custom data

    [BsonElement("isRead")]
    public bool IsRead { get; set; } = false;

    [BsonElement("readAt")]
    public DateTime? ReadAt { get; set; }

    [BsonElement("deliveryStatus")]
    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.Pending;

    [BsonElement("deliveryChannels")]
    public List<DeliveryChannel> DeliveryChannels { get; set; } = new();

    [BsonElement("priority")]
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;

    [BsonElement("expiresAt")]
    public DateTime? ExpiresAt { get; set; }

    [BsonElement("createdAt")]
    public override DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public override DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum NotificationType
{
    // Social interactions
    Like,
    Comment,
    Reply,
    Mention,
    Share,
    Reaction,

    // Follows
    Follow,
    FollowRequest,
    FollowRequestAccepted,

    // Messages
    NewMessage,
    MessageReaction,
    GroupInvite,

    // Videos
    VideoLike,
    VideoComment,
    VideoShare,
    NewVideoFromSubscription,

    // Posts
    PostLike,
    PostComment,
    PostShare,
    PostMention,

    // System
    Welcome,
    AccountUpdate,
    SecurityAlert,
    SystemAnnouncement,

    // Achievements
    Milestone,
    Badge
}

public enum EntityType
{
    Post,
    Comment,
    Video,
    Message,
    Conversation,
    User,
    None
}

public enum DeliveryStatus
{
    Pending,
    Delivered,
    Failed,
    Expired
}

public enum DeliveryChannel
{
    InApp,
    Push,
    Email,
    SMS
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Critical
}

public class NotificationPreferences : Entity<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public override string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonElement("enableInApp")]
    public bool EnableInApp { get; set; } = true;

    [BsonElement("enablePush")]
    public bool EnablePush { get; set; } = true;

    [BsonElement("enableEmail")]
    public bool EnableEmail { get; set; } = true;

    [BsonElement("enableSMS")]
    public bool EnableSMS { get; set; } = false;

    [BsonElement("notificationTypes")]
    public Dictionary<NotificationType, NotificationChannelSettings> NotificationTypes { get; set; } = new();

    [BsonElement("muteUntil")]
    public DateTime? MuteUntil { get; set; }

    [BsonElement("quietHoursStart")]
    public TimeSpan? QuietHoursStart { get; set; } // e.g., 22:00

    [BsonElement("quietHoursEnd")]
    public TimeSpan? QuietHoursEnd { get; set; } // e.g., 08:00

    [BsonElement("createdAt")]
    public override DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public override DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class NotificationChannelSettings
{
    [BsonElement("inApp")]
    public bool InApp { get; set; } = true;

    [BsonElement("push")]
    public bool Push { get; set; } = true;

    [BsonElement("email")]
    public bool Email { get; set; } = false;

    [BsonElement("sms")]
    public bool SMS { get; set; } = false;
}

public class DeviceToken : Entity<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public override string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonElement("token")]
    public string Token { get; set; } = string.Empty;

    [BsonElement("platform")]
    public DevicePlatform Platform { get; set; }

    [BsonElement("deviceId")]
    public string? DeviceId { get; set; }

    [BsonElement("deviceName")]
    public string? DeviceName { get; set; }

    [BsonElement("appVersion")]
    public string? AppVersion { get; set; }

    [BsonElement("isActive")]
    public bool IsActive { get; set; } = true;

    [BsonElement("lastUsedAt")]
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("createdAt")]
    public override DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public override DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum DevicePlatform
{
    iOS,
    Android,
    Web
}
