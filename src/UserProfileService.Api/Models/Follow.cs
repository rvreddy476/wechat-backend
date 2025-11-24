using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Domain.Common;

namespace UserProfileService.Api.Models;

public class Follow : Entity<string>, ISoftDelete
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public override string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("followerId")]
    [BsonRepresentation(BsonType.String)]
    public Guid FollowerId { get; set; }

    [BsonElement("followeeId")]
    [BsonRepresentation(BsonType.String)]
    public Guid FolloweeId { get; set; }

    [BsonElement("status")]
    public FollowStatus Status { get; set; } = FollowStatus.Accepted;

    [BsonElement("createdAt")]
    public override DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public override DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    [BsonElement("deletedAt")]
    public DateTime? DeletedAt { get; set; }
}

public enum FollowStatus
{
    Pending,
    Accepted,
    Rejected
}
