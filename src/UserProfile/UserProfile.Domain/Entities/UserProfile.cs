using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Domain.Common;

namespace UserProfile.Domain.Entities;

public class UserProfile : AggregateRoot
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("displayName")]
    public string DisplayName { get; set; } = string.Empty;

    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;

    [BsonElement("bio")]
    public string? Bio { get; set; }

    [BsonElement("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [BsonElement("coverPhotoUrl")]
    public string? CoverPhotoUrl { get; set; }

    [BsonElement("friends")]
    public List<Guid> Friends { get; set; } = new();

    [BsonElement("followers")]
    public List<Guid> Followers { get; set; } = new();

    [BsonElement("following")]
    public List<Guid> Following { get; set; } = new();

    [BsonElement("isOnline")]
    public bool IsOnline { get; set; } = false;

    [BsonElement("lastSeenAt")]
    public DateTime? LastSeenAt { get; set; }
}
