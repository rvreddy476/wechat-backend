using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shared.Domain.Common;

namespace MediaService.Api.Models;

public class Media : Entity<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public override string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonElement("filename")]
    public string Filename { get; set; } = string.Empty;

    [BsonElement("originalFilename")]
    public string OriginalFilename { get; set; } = string.Empty;

    [BsonElement("contentType")]
    public string ContentType { get; set; } = string.Empty;

    [BsonElement("mediaType")]
    public MediaType MediaType { get; set; }

    [BsonElement("fileSize")]
    public long FileSize { get; set; }

    [BsonElement("url")]
    public string Url { get; set; } = string.Empty;

    [BsonElement("cdnUrl")]
    public string? CdnUrl { get; set; }

    [BsonElement("thumbnailUrl")]
    public string? ThumbnailUrl { get; set; }

    [BsonElement("width")]
    public int? Width { get; set; }

    [BsonElement("height")]
    public int? Height { get; set; }

    [BsonElement("duration")]
    public int? Duration { get; set; } // in seconds for video/audio

    [BsonElement("metadata")]
    public MediaMetadata Metadata { get; set; } = new();

    [BsonElement("storageProvider")]
    public StorageProvider StorageProvider { get; set; }

    [BsonElement("storagePath")]
    public string StoragePath { get; set; } = string.Empty;

    [BsonElement("status")]
    public MediaStatus Status { get; set; } = MediaStatus.Processing;

    [BsonElement("uploadedAt")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("processedAt")]
    public DateTime? ProcessedAt { get; set; }

    [BsonElement("expiresAt")]
    public DateTime? ExpiresAt { get; set; }

    [BsonElement("isPublic")]
    public bool IsPublic { get; set; } = false;

    [BsonElement("tags")]
    public List<string> Tags { get; set; } = new();

    [BsonElement("altText")]
    public string? AltText { get; set; }

    [BsonElement("entityType")]
    public string? EntityType { get; set; } // Post, Comment, Message, Profile, etc.

    [BsonElement("entityId")]
    public string? EntityId { get; set; }

    [BsonElement("createdAt")]
    public override DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public override DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum MediaType
{
    Image,
    Video,
    Audio,
    Document,
    Other
}

public enum StorageProvider
{
    Local,
    GoogleCloudStorage,
    AmazonS3,
    AzureBlobStorage
}

public enum MediaStatus
{
    Processing,
    Ready,
    Failed,
    Deleted
}

public class MediaMetadata
{
    [BsonElement("format")]
    public string? Format { get; set; }

    [BsonElement("codec")]
    public string? Codec { get; set; }

    [BsonElement("bitrate")]
    public int? Bitrate { get; set; }

    [BsonElement("fps")]
    public int? Fps { get; set; }

    [BsonElement("channels")]
    public int? Channels { get; set; }

    [BsonElement("sampleRate")]
    public int? SampleRate { get; set; }

    [BsonElement("exifData")]
    public Dictionary<string, string>? ExifData { get; set; }

    [BsonElement("location")]
    public MediaLocation? Location { get; set; }
}

public class MediaLocation
{
    [BsonElement("latitude")]
    public double Latitude { get; set; }

    [BsonElement("longitude")]
    public double Longitude { get; set; }

    [BsonElement("altitude")]
    public double? Altitude { get; set; }
}

public class Upload : Entity<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public override string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.String)]
    public Guid UserId { get; set; }

    [BsonElement("uploadKey")]
    public string UploadKey { get; set; } = string.Empty;

    [BsonElement("filename")]
    public string Filename { get; set; } = string.Empty;

    [BsonElement("contentType")]
    public string ContentType { get; set; } = string.Empty;

    [BsonElement("fileSize")]
    public long FileSize { get; set; }

    [BsonElement("chunkSize")]
    public long ChunkSize { get; set; }

    [BsonElement("totalChunks")]
    public int TotalChunks { get; set; }

    [BsonElement("uploadedChunks")]
    public List<int> UploadedChunks { get; set; } = new();

    [BsonElement("status")]
    public UploadStatus Status { get; set; } = UploadStatus.InProgress;

    [BsonElement("mediaId")]
    public string? MediaId { get; set; }

    [BsonElement("expiresAt")]
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(24);

    [BsonElement("createdAt")]
    public override DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public override DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum UploadStatus
{
    InProgress,
    Completed,
    Failed,
    Expired
}

public class MediaProcessingJob : Entity<string>
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public override string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonElement("mediaId")]
    public string MediaId { get; set; } = string.Empty;

    [BsonElement("jobType")]
    public ProcessingJobType JobType { get; set; }

    [BsonElement("status")]
    public ProcessingStatus Status { get; set; } = ProcessingStatus.Queued;

    [BsonElement("progress")]
    public int Progress { get; set; } // 0-100

    [BsonElement("error")]
    public string? Error { get; set; }

    [BsonElement("startedAt")]
    public DateTime? StartedAt { get; set; }

    [BsonElement("completedAt")]
    public DateTime? CompletedAt { get; set; }

    [BsonElement("retryCount")]
    public int RetryCount { get; set; } = 0;

    [BsonElement("maxRetries")]
    public int MaxRetries { get; set; } = 3;

    [BsonElement("createdAt")]
    public override DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public override DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum ProcessingJobType
{
    ImageOptimization,
    ThumbnailGeneration,
    VideoTranscoding,
    AudioTranscoding,
    MetadataExtraction
}

public enum ProcessingStatus
{
    Queued,
    Processing,
    Completed,
    Failed
}
