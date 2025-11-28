using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Domain.Common;

namespace ChatService.Domain.Entities;

public class Conversation : AggregateRoot, ISoftDelete
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

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

    public void UpdateLastMessage(string messageId, Guid senderId, string senderUsername, string content, MessageType messageType)
    {
        LastMessage = new LastMessage
        {
            MessageId = messageId,
            SenderId = senderId,
            SenderUsername = senderUsername,
            Content = content,
            MessageType = messageType,
            SentAt = DateTime.UtcNow
        };
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddParticipant(Guid userId, string username)
    {
        if (!Participants.Any(p => p.UserId == userId))
        {
            Participants.Add(new Participant
            {
                UserId = userId,
                Username = username,
                JoinedAt = DateTime.UtcNow
            });
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void RemoveParticipant(Guid userId)
    {
        Participants.RemoveAll(p => p.UserId == userId);
        Admins.Remove(userId);
        UpdatedAt = DateTime.UtcNow;
    }

    public void MuteForParticipant(Guid userId, DateTime? mutedUntil)
    {
        var participant = Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant != null)
        {
            participant.IsMuted = mutedUntil.HasValue;
            participant.MutedUntil = mutedUntil;
        }
    }

    public void MarkAsRead(Guid userId, string messageId)
    {
        var participant = Participants.FirstOrDefault(p => p.UserId == userId);
        if (participant != null)
        {
            participant.LastReadAt = DateTime.UtcNow;
            participant.LastReadMessageId = messageId;
        }
    }
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
