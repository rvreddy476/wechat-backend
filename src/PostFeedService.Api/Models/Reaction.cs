using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Domain.Common;

namespace PostFeedService.Api.Models;

public class Reaction : Entity<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public override string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("targetId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string TargetId { get; set; } = string.Empty; // Post or Comment ID

    [BsonElement("targetType")]
    public ReactionTargetType TargetType { get; set; }

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonElement("reactionType")]
    public ReactionType ReactionType { get; set; }

    [BsonElement("createdAt")]
    public override DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public override DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum ReactionTargetType
{
    Post,
    Comment
}

public enum ReactionType
{
    Like,
    Love,
    Haha,
    Wow,
    Sad,
    Angry
}
