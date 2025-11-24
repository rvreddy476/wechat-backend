using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Domain.Common;

namespace ChatService.Api.Models;

public class Message : Entity<string>, ISoftDelete
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public override string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("conversationId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ConversationId { get; set; } = string.Empty;

    [BsonElement("senderId")]
    [BsonRepresentation(BsonType.String)]
    public Guid SenderId { get; set; }

    [BsonElement("senderUsername")]
    public string SenderUsername { get; set; } = string.Empty;

    [BsonElement("messageType")]
    public MessageType MessageType { get; set; }

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("mediaUrl")]
    public string? MediaUrl { get; set; }

    [BsonElement("mediaThumbnailUrl")]
    public string? MediaThumbnailUrl { get; set; }

    [BsonElement("mediaDuration")]
    public int? MediaDuration { get; set; }

    [BsonElement("fileName")]
    public string? FileName { get; set; }

    [BsonElement("fileSize")]
    public long? FileSize { get; set; }

    [BsonElement("location")]
    public MessageLocation? Location { get; set; }

    [BsonElement("replyToMessageId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ReplyToMessageId { get; set; }

    [BsonElement("forwardedFromMessageId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ForwardedFromMessageId { get; set; }

    [BsonElement("mentions")]
    public List<Guid> Mentions { get; set; } = new();

    [BsonElement("readBy")]
    public List<MessageReadReceipt> ReadBy { get; set; } = new();

    [BsonElement("deliveredAt")]
    public DateTime? DeliveredAt { get; set; }

    [BsonElement("isEdited")]
    public bool IsEdited { get; set; } = false;

    [BsonElement("editedAt")]
    public DateTime? EditedAt { get; set; }

    [BsonElement("createdAt")]
    public override DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public override DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    [BsonElement("deletedAt")]
    public DateTime? DeletedAt { get; set; }

    [BsonElement("deletedFor")]
    public List<Guid> DeletedFor { get; set; } = new(); // For "delete for me" feature
}

public class MessageLocation
{
    [BsonElement("latitude")]
    public double Latitude { get; set; }

    [BsonElement("longitude")]
    public double Longitude { get; set; }

    [BsonElement("name")]
    public string? Name { get; set; }

    [BsonElement("address")]
    public string? Address { get; set; }
}

public class MessageReadReceipt
{
    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonElement("readAt")]
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;
}

public enum MessageType
{
    Text,
    Image,
    Video,
    Audio,
    File,
    Location,
    Contact,
    Sticker,
    Gif,
    System
}
