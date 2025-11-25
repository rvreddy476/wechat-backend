using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Domain.Common;

namespace ChatService.Api.Models;

public class Conversation : Entity<string>, ISoftDelete
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public override string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("type")]
    public ConversationType Type { get; set; }

    [BsonElement("participants")]
    public List<Participant> Participants { get; set; } = new();

    [BsonElement("groupName")]
    public string? GroupName { get; set; }

    [BsonElement("groupAvatarUrl")]
    public string? GroupAvatarUrl { get; set; }

    [BsonElement("groupDescription")]
    public string? GroupDescription { get; set; }

    [BsonElement("createdBy")]
    [BsonRepresentation(BsonType.String)]
    public Guid CreatedBy { get; set; }

    [BsonElement("admins")]
    public List<Guid> Admins { get; set; } = new();

    [BsonElement("lastMessage")]
    public LastMessage? LastMessage { get; set; }

    [BsonElement("createdAt")]
    public override DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public override DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    [BsonElement("deletedAt")]
    public DateTime? DeletedAt { get; set; }
}

public class Participant
{
    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("joinedAt")]
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("lastReadAt")]
    public DateTime? LastReadAt { get; set; }

    [BsonElement("lastReadMessageId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? LastReadMessageId { get; set; }

    [BsonElement("isMuted")]
    public bool IsMuted { get; set; } = false;

    [BsonElement("mutedUntil")]
    public DateTime? MutedUntil { get; set; }
}

public class LastMessage
{
    [BsonElement("messageId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string MessageId { get; set; } = string.Empty;

    [BsonElement("senderId")]
    [BsonRepresentation(BsonType.String)]
    public Guid SenderId { get; set; }

    [BsonElement("senderUsername")]
    public string SenderUsername { get; set; } = string.Empty;

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("messageType")]
    public MessageType MessageType { get; set; }

    [BsonElement("sentAt")]
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

public enum ConversationType
{
    OneToOne,
    Group
}
