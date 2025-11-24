using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Domain.Common;

namespace UserProfileService.Api.Models;

public class BlockedUser : Entity<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public override string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonElement("blockedUserId")]
    [BsonRepresentation(BsonType.String)]
    public Guid BlockedUserId { get; set; }

    [BsonElement("reason")]
    public string? Reason { get; set; }

    [BsonElement("createdAt")]
    public override DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public override DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
