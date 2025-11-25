using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Domain.Common;

namespace PostFeedService.Api.Models;

public class Post : Entity<string>, ISoftDelete
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public override string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("content")]
    public string Content { get; set; } = string.Empty;

    [BsonElement("contentType")]
    public PostContentType ContentType { get; set; } = PostContentType.Text;

    [BsonElement("mediaUrls")]
    public List<MediaItem> MediaUrls { get; set; } = new();

    [BsonElement("poll")]
    public Poll? Poll { get; set; }

    [BsonElement("location")]
    public Location? Location { get; set; }

    [BsonElement("mentions")]
    public List<Guid> Mentions { get; set; } = new();

    [BsonElement("hashtags")]
    public List<string> Hashtags { get; set; } = new();

    [BsonElement("visibility")]
    public PostVisibility Visibility { get; set; } = PostVisibility.Public;

    [BsonElement("isCommentsEnabled")]
    public bool IsCommentsEnabled { get; set; } = true;

    [BsonElement("isPinned")]
    public bool IsPinned { get; set; } = false;

    [BsonElement("sharedFromPostId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? SharedFromPostId { get; set; }

    [BsonElement("stats")]
    public PostStats Stats { get; set; } = new();

    [BsonElement("createdAt")]
    public override DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public override DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    [BsonElement("deletedAt")]
    public DateTime? DeletedAt { get; set; }
}

public class MediaItem
{
    [BsonElement("url")]
    public string Url { get; set; } = string.Empty;

    [BsonElement("type")]
    public MediaType Type { get; set; }

    [BsonElement("width")]
    public int? Width { get; set; }

    [BsonElement("height")]
    public int? Height { get; set; }

    [BsonElement("duration")]
    public int? Duration { get; set; }

    [BsonElement("thumbnailUrl")]
    public string? ThumbnailUrl { get; set; }
}

public class Poll
{
    [BsonElement("question")]
    public string Question { get; set; } = string.Empty;

    [BsonElement("options")]
    public List<PollOption> Options { get; set; } = new();

    [BsonElement("endsAt")]
    public DateTime EndsAt { get; set; }

    [BsonElement("allowMultipleChoices")]
    public bool AllowMultipleChoices { get; set; } = false;
}

public class PollOption
{
    [BsonElement("optionText")]
    public string OptionText { get; set; } = string.Empty;

    [BsonElement("votes")]
    public int Votes { get; set; } = 0;

    [BsonElement("votedBy")]
    public List<Guid> VotedBy { get; set; } = new();
}

public class Location
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("latitude")]
    public double Latitude { get; set; }

    [BsonElement("longitude")]
    public double Longitude { get; set; }
}

public class PostStats
{
    [BsonElement("likesCount")]
    public int LikesCount { get; set; } = 0;

    [BsonElement("commentsCount")]
    public int CommentsCount { get; set; } = 0;

    [BsonElement("sharesCount")]
    public int SharesCount { get; set; } = 0;

    [BsonElement("viewsCount")]
    public int ViewsCount { get; set; } = 0;

    [BsonElement("reactionsCount")]
    public Dictionary<string, int> ReactionsCount { get; set; } = new();
}

public enum PostContentType
{
    Text,
    Image,
    Video,
    Poll,
    Shared
}

public enum MediaType
{
    Image,
    Video,
    Gif
}

public enum PostVisibility
{
    Public,
    Followers,
    Private
}
