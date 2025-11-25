namespace VideoProcessing.Worker.Models;

/// <summary>
/// Represents a video processing job
/// </summary>
public class VideoProcessingJob
{
    public string VideoId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string SourceUrl { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public VideoProcessingOptions Options { get; set; } = new();
    public DateTime EnqueuedAt { get; set; } = DateTime.UtcNow;
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// Processing options for video transcoding
/// </summary>
public class VideoProcessingOptions
{
    /// <summary>
    /// Generate multiple quality variants (1080p, 720p, 480p, 360p)
    /// </summary>
    public bool GenerateQualityVariants { get; set; } = true;

    /// <summary>
    /// Generate HLS streaming manifest
    /// </summary>
    public bool GenerateHLS { get; set; } = true;

    /// <summary>
    /// Number of thumbnails to generate
    /// </summary>
    public int ThumbnailCount { get; set; } = 5;

    /// <summary>
    /// Extract video metadata
    /// </summary>
    public bool ExtractMetadata { get; set; } = true;

    /// <summary>
    /// Target video codec (h264, h265, vp9)
    /// </summary>
    public string TargetCodec { get; set; } = "h264";

    /// <summary>
    /// Target audio codec (aac, opus)
    /// </summary>
    public string TargetAudioCodec { get; set; } = "aac";

    /// <summary>
    /// Quality variants to generate
    /// </summary>
    public List<QualityPreset> QualityPresets { get; set; } = new()
    {
        new QualityPreset { Name = "1080p", Width = 1920, Height = 1080, VideoBitrate = 4500, AudioBitrate = 192 },
        new QualityPreset { Name = "720p", Width = 1280, Height = 720, VideoBitrate = 2500, AudioBitrate = 128 },
        new QualityPreset { Name = "480p", Width = 854, Height = 480, VideoBitrate = 1000, AudioBitrate = 96 },
        new QualityPreset { Name = "360p", Width = 640, Height = 360, VideoBitrate = 600, AudioBitrate = 64 },
    };
}

/// <summary>
/// Quality preset for video transcoding
/// </summary>
public class QualityPreset
{
    public string Name { get; set; } = string.Empty; // 1080p, 720p, etc.
    public int Width { get; set; }
    public int Height { get; set; }
    public int VideoBitrate { get; set; } // kbps
    public int AudioBitrate { get; set; } // kbps
}

/// <summary>
/// Result of video processing
/// </summary>
public class VideoProcessingResult
{
    public string VideoId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public VideoMetadataResult? Metadata { get; set; }
    public List<QualityVariantResult> QualityVariants { get; set; } = new();
    public List<string> ThumbnailUrls { get; set; } = new();
    public string? StreamingUrl { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

/// <summary>
/// Extracted video metadata
/// </summary>
public class VideoMetadataResult
{
    public int DurationSeconds { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string AspectRatio { get; set; } = "16:9";
    public double FrameRate { get; set; }
    public string VideoCodec { get; set; } = string.Empty;
    public int VideoBitrate { get; set; } // kbps
    public string AudioCodec { get; set; } = string.Empty;
    public int AudioBitrate { get; set; } // kbps
    public int AudioChannels { get; set; }
    public long FileSize { get; set; } // bytes
}

/// <summary>
/// Result of transcoding a quality variant
/// </summary>
public class QualityVariantResult
{
    public string Quality { get; set; } = string.Empty; // 1080p, 720p, etc.
    public string Url { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public int Bitrate { get; set; }
    public string Codec { get; set; } = "h264";
}
