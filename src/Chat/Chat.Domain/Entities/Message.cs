using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Domain.Common;

namespace Chat.Domain.Entities;

public class Message : AggregateRoot, ISoftDelete
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("conversationId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string ConversationId { get; set; } = string.Empty;

    [BsonElement("senderId")]
    [BsonRepresentation(BsonType.String)]
    public Guid SenderId { get; set; }

    [BsonElement("senderUsername")]
    public string SenderUsername { get; set; } = string.Empty;

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("messageType")]
    public MessageType MessageType { get; set; } = MessageType.Text;

    [BsonElement("mediaUrl")]
    public string? MediaUrl { get; set; }

    [BsonElement("replyToMessageId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ReplyToMessageId { get; set; }

    [BsonElement("readBy")]
    public List<MessageReadReceipt> ReadBy { get; set; } = new();

    [BsonElement("isEdited")]
    public bool IsEdited { get; set; } = false;

    [BsonElement("editedAt")]
    public DateTime? EditedAt { get; set; }

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    [BsonElement("deletedAt")]
    public DateTime? DeletedAt { get; set; }

    public void MarkAsRead(Guid userId)
    {
        if (!ReadBy.Any(r => r.UserId == userId))
        {
            ReadBy.Add(new MessageReadReceipt { UserId = userId, ReadAt = DateTime.UtcNow });
        }
    }

    public void Edit(string newContent)
    {
        Content = newContent;
        IsEdited = true;
        EditedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
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
    File
}
