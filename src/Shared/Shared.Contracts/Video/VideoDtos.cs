namespace Shared.Contracts.Video;

/// <summary>
/// Video DTO
/// </summary>
public record VideoDto
{
    public string Id { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string OriginalUrl { get; init; } = string.Empty;
    public string? ProcessedUrl { get; init; }
    public string ThumbnailUrl { get; init; } = string.Empty;
    public VideoProcessingStatus Status { get; init; }
    public long SizeInBytes { get; init; }
    public int DurationSeconds { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public List<VideoQualityDto> AvailableQualities { get; init; } = new();
    public DateTime UploadedAt { get; init; }
    public DateTime? ProcessedAt { get; init; }
}

/// <summary>
/// Video quality DTO
/// </summary>
public record VideoQualityDto
{
    public VideoQuality Quality { get; init; }
    public string Url { get; init; } = string.Empty;
    public int Width { get; init; }
    public int Height { get; init; }
    public int BitrateKbps { get; init; }
    public long SizeInBytes { get; init; }
}

/// <summary>
/// Video upload request
/// </summary>
public record VideoUploadRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string FileName { get; init; } = string.Empty;
    public long SizeInBytes { get; init; }
}

/// <summary>
/// Video processing request
/// </summary>
public record VideoProcessingRequest
{
    public string VideoId { get; init; } = string.Empty;
    public List<VideoQuality> TargetQualities { get; init; } = new();
    public bool GenerateThumbnails { get; init; } = true;
    public int ThumbnailCount { get; init; } = 3;
}

/// <summary>
/// Video processing status enumeration
/// </summary>
public enum VideoProcessingStatus
{
    Uploaded,
    Processing,
    Completed,
    Failed
}

/// <summary>
/// Video quality enumeration
/// </summary>
public enum VideoQuality
{
    SD_360p,
    SD_480p,
    HD_720p,
    HD_1080p,
    UHD_4K
}
