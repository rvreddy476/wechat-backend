using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Domain.Common;

namespace PostFeedService.Api.Models;

public class Hashtag : Entity<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public override string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("tag")]
    public string Tag { get; set; } = string.Empty;

    [BsonElement("normalizedTag")]
    public string NormalizedTag { get; set; } = string.Empty;

    [BsonElement("usageCount")]
    public int UsageCount { get; set; } = 0;

    [BsonElement("trendingScore")]
    public double TrendingScore { get; set; } = 0;

    [BsonElement("lastUsedAt")]
    public DateTime LastUsedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("createdAt")]
    public override DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public override DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
