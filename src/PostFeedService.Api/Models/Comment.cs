using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Domain.Common;

namespace PostFeedService.Api.Models;

public class Comment : Entity<string>, ISoftDelete
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public override string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("postId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string PostId { get; set; } = string.Empty;

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("parentCommentId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ParentCommentId { get; set; }

    [BsonElement("level")]
    public int Level { get; set; } = 0; // 0 = top-level, max 5

    [BsonElement("mentions")]
    public List<Guid> Mentions { get; set; } = new();

    [BsonElement("mediaUrl")]
    public string? MediaUrl { get; set; }

    [BsonElement("likesCount")]
    public int LikesCount { get; set; } = 0;

    [BsonElement("repliesCount")]
    public int RepliesCount { get; set; } = 0;

    [BsonElement("createdAt")]
    public override DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public override DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    [BsonElement("deletedAt")]
    public DateTime? DeletedAt { get; set; }
}
