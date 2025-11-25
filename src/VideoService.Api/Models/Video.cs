using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Domain.Common;

namespace VideoService.Api.Models;

public class Video : Entity<string>, ISoftDelete
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public override string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonElement("username")]
    public string Username { get; set; } = string.Empty;

    [BsonElement("title")]
    public string Title { get; set; } = string.Empty;

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("videoType")]
    public VideoType VideoType { get; set; }

    [BsonElement("duration")]
    public int Duration { get; set; } // in seconds

    [BsonElement("originalFileName")]
    public string OriginalFileName { get; set; } = string.Empty;

    [BsonElement("fileSize")]
    public long FileSize { get; set; } // in bytes

    [BsonElement("format")]
    public string Format { get; set; } = string.Empty; // mp4, mov, etc.

    [BsonElement("resolution")]
    public VideoResolution Resolution { get; set; } = new();

    [BsonElement("processingStatus")]
    public ProcessingStatus ProcessingStatus { get; set; } = ProcessingStatus.Uploaded;

    [BsonElement("processingProgress")]
    public int ProcessingProgress { get; set; } = 0; // 0-100

    [BsonElement("processingError")]
    public string? ProcessingError { get; set; }

    [BsonElement("sourceUrl")]
    public string? SourceUrl { get; set; } // Original upload URL

    [BsonElement("streamingUrl")]
    public string? StreamingUrl { get; set; } // HLS manifest URL

    [BsonElement("thumbnailUrls")]
    public List<string> ThumbnailUrls { get; set; } = new();

    [BsonElement("selectedThumbnailIndex")]
    public int SelectedThumbnailIndex { get; set; } = 0;

    [BsonElement("qualityVariants")]
    public List<QualityVariant> QualityVariants { get; set; } = new();

    [BsonElement("visibility")]
    public VideoVisibility Visibility { get; set; } = VideoVisibility.Public;

    [BsonElement("category")]
    public string? Category { get; set; }

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("hashtags")]
    public List<string> Hashtags { get; set; } = new();

    [BsonElement("mentions")]
    public List<Guid> Mentions { get; set; } = new();

    [BsonElement("isCommentsEnabled")]
    public bool IsCommentsEnabled { get; set; } = true;

    [BsonElement("ageRestricted")]
    public bool AgeRestricted { get; set; } = false;

    [BsonElement("isFeatured")]
    public bool IsFeatured { get; set; } = false;

    [BsonElement("stats")]
    public VideoStats Stats { get; set; } = new();

    [BsonElement("metadata")]
    public VideoMetadata Metadata { get; set; } = new();

    [BsonElement("createdAt")]
    public override DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public override DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("publishedAt")]
    public DateTime? PublishedAt { get; set; }

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; } = false;

    [BsonElement("deletedAt")]
    public DateTime? DeletedAt { get; set; }
}

public class VideoResolution
{
    [BsonElement("width")]
    public int Width { get; set; }

    [BsonElement("height")]
    public int Height { get; set; }

    [BsonElement("aspectRatio")]
    public string AspectRatio { get; set; } = "16:9";
}

public class QualityVariant
{
    [BsonElement("quality")]
    public string Quality { get; set; } = string.Empty; // 360p, 480p, 720p, 1080p, 4K

    [BsonElement("url")]
    public string Url { get; set; } = string.Empty;

    [BsonElement("fileSize")]
    public long FileSize { get; set; }

    [BsonElement("bitrate")]
    public int Bitrate { get; set; } // kbps

    [BsonElement("codec")]
    public string Codec { get; set; } = "h264";
}

public class VideoStats
{
    [BsonElement("viewsCount")]
    public long ViewsCount { get; set; } = 0;

    [BsonElement("uniqueViewsCount")]
    public long UniqueViewsCount { get; set; } = 0;

    [BsonElement("likesCount")]
    public int LikesCount { get; set; } = 0;

    [BsonElement("dislikesCount")]
    public int DislikesCount { get; set; } = 0;

    [BsonElement("commentsCount")]
    public int CommentsCount { get; set; } = 0;

    [BsonElement("sharesCount")]
    public int SharesCount { get; set; } = 0;

    [BsonElement("watchTimeSeconds")]
    public long WatchTimeSeconds { get; set; } = 0;

    [BsonElement("averageWatchPercentage")]
    public double AverageWatchPercentage { get; set; } = 0;

    [BsonElement("completionRate")]
    public double CompletionRate { get; set; } = 0; // Percentage who watched to the end
}

public class VideoMetadata
{
    [BsonElement("codec")]
    public string? Codec { get; set; }

    [BsonElement("bitrate")]
    public int? Bitrate { get; set; }

    [BsonElement("frameRate")]
    public double? FrameRate { get; set; }

    [BsonElement("audioCodec")]
    public string? AudioCodec { get; set; }

    [BsonElement("audioBitrate")]
    public int? AudioBitrate { get; set; }

    [BsonElement("audioChannels")]
    public int? AudioChannels { get; set; }

    [BsonElement("uploadedFrom")]
    public string? UploadedFrom { get; set; } // Device type, browser, etc.

    [BsonElement("location")]
    public VideoLocation? Location { get; set; }
}

public class VideoLocation
{
    [BsonElement("latitude")]
    public double Latitude { get; set; }

    [BsonElement("longitude")]
    public double Longitude { get; set; }

    [BsonElement("name")]
    public string? Name { get; set; }

    [BsonElement("city")]
    public string? City { get; set; }

    [BsonElement("country")]
    public string? Country { get; set; }
}

public enum VideoType
{
    LongForm,
    Short
}

public enum ProcessingStatus
{
    Uploaded,
    Processing,
    Ready,
    Failed
}

public enum VideoVisibility
{
    Public,
    Unlisted,
    Private
}
