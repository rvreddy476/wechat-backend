namespace Shared.Contracts.Chat;

/// <summary>
/// Chat message DTO
/// </summary>
public record ChatMessageDto
{
    public string Id { get; init; } = string.Empty;
    public string ConversationId { get; init; } = string.Empty;
    public string SenderId { get; init; } = string.Empty;
    public string SenderUsername { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public MessageType Type { get; init; }
    public DateTime SentAt { get; init; }
    public bool IsRead { get; init; }
    public DateTime? ReadAt { get; init; }
    public bool IsEdited { get; init; }
    public DateTime? EditedAt { get; init; }
    public List<string>? AttachmentUrls { get; init; }
    public string? ReplyToMessageId { get; init; }
}

/// <summary>
/// Conversation DTO
/// </summary>
public record ConversationDto
{
    public string Id { get; init; } = string.Empty;
    public ConversationType Type { get; init; }
    public string? Name { get; init; }
    public string? AvatarUrl { get; init; }
    public List<string> ParticipantIds { get; init; } = new();
    public ChatMessageDto? LastMessage { get; init; }
    public int UnreadCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// Request to send a message
/// </summary>
public record SendMessageRequest
{
    public string ConversationId { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public MessageType Type { get; init; } = MessageType.Text;
    public List<string>? AttachmentUrls { get; init; }
    public string? ReplyToMessageId { get; init; }
}

/// <summary>
/// Request to create a conversation
/// </summary>
public record CreateConversationRequest
{
    public ConversationType Type { get; init; }
    public string? Name { get; init; }
    public List<string> ParticipantIds { get; init; } = new();
}

/// <summary>
/// Request to mark messages as read
/// </summary>
public record MarkMessagesAsReadRequest
{
    public string ConversationId { get; init; } = string.Empty;
    public List<string> MessageIds { get; init; } = new();
}

/// <summary>
/// Message type enumeration
/// </summary>
public enum MessageType
{
    Text,
    Image,
    Video,
    Audio,
    File,
    Location,
    Contact
}

/// <summary>
/// Conversation type enumeration
/// </summary>
public enum ConversationType
{
    OneToOne,
    Group
}
