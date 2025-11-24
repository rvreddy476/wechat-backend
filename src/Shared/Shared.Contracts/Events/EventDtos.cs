namespace Shared.Contracts.Events;

/// <summary>
/// Base event for Phase 1 HTTP-based events
/// </summary>
public abstract record DomainEventDto
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

// User Events
public record UserRegisteredEvent : DomainEventDto
{
    public required string UserId { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
}

public record UserFollowedEvent : DomainEventDto
{
    public required string FollowerId { get; init; }
    public required string FollowingId { get; init; }
}

// Post Events
public record PostCreatedEvent : DomainEventDto
{
    public required string PostId { get; init; }
    public required string UserId { get; init; }
    public List<string> MentionedUserIds { get; init; } = new();
    public List<string> Hashtags { get; init; } = new();
}

public record CommentCreatedEvent : DomainEventDto
{
    public required string CommentId { get; init; }
    public required string PostId { get; init; }
    public required string UserId { get; init; }
    public required string PostOwnerId { get; init; }
    public List<string> MentionedUserIds { get; init; } = new();
}

public record ReactionAddedEvent : DomainEventDto
{
    public required string EntityType { get; init; } // "post" or "comment"
    public required string EntityId { get; init; }
    public required string UserId { get; init; }
    public required string EntityOwnerId { get; init; }
    public required string ReactionType { get; init; }
}

// Chat Events
public record MessageSentEvent : DomainEventDto
{
    public required string MessageId { get; init; }
    public required string ConversationId { get; init; }
    public required string SenderId { get; init; }
    public List<string> RecipientIds { get; init; } = new();
}

// Video Events
public record VideoUploadedEvent : DomainEventDto
{
    public required string VideoId { get; init; }
    public required string UserId { get; init; }
    public required string VideoType { get; init; } // "long-form" or "short"
}

public record VideoProcessingCompletedEvent : DomainEventDto
{
    public required string VideoId { get; init; }
    public required string UserId { get; init; }
    public bool Success { get; init; }
}
