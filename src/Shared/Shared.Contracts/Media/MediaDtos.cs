namespace Shared.Contracts.Media;

/// <summary>
/// Media upload response DTO
/// </summary>
public record MediaUploadResponse
{
    public string Id { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string ThumbnailUrl { get; init; } = string.Empty;
    public MediaType Type { get; init; }
    public long SizeInBytes { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
}

/// <summary>
/// Media metadata DTO
/// </summary>
public record MediaMetadataDto
{
    public string Id { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public MediaType Type { get; init; }
    public long SizeInBytes { get; init; }
    public string ContentType { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public string? ThumbnailUrl { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
    public DateTime UploadedAt { get; init; }
    public DateTime ExpiresAt { get; init; }
}

/// <summary>
/// Media upload request
/// </summary>
public record MediaUploadRequest
{
    public string FileName { get; init; } = string.Empty;
    public MediaType Type { get; init; }
    public long SizeInBytes { get; init; }
}

/// <summary>
/// Image processing request
/// </summary>
public record ImageProcessingRequest
{
    public string MediaId { get; init; } = string.Empty;
    public int? Width { get; init; }
    public int? Height { get; init; }
    public ImageFormat Format { get; init; }
    public int Quality { get; init; } = 85;
}

/// <summary>
/// Media type enumeration
/// </summary>
public enum MediaType
{
    Image,
    Video,
    Audio,
    Document
}

/// <summary>
/// Image format enumeration
/// </summary>
public enum ImageFormat
{
    Jpeg,
    Png,
    Webp,
    Gif
}
