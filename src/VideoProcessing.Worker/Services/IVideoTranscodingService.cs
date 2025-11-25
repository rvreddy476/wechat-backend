using VideoProcessing.Worker.Models;

namespace VideoProcessing.Worker.Services;

/// <summary>
/// Service for video transcoding and processing
/// </summary>
public interface IVideoTranscodingService
{
    /// <summary>
    /// Extract metadata from video file
    /// </summary>
    Task<VideoMetadataResult?> ExtractMetadataAsync(string inputFilePath);

    /// <summary>
    /// Transcode video to a specific quality variant
    /// </summary>
    Task<string> TranscodeToQualityAsync(string inputFilePath, string outputFilePath, QualityPreset preset, IProgress<int>? progress = null);

    /// <summary>
    /// Generate HLS streaming format with multiple quality variants
    /// </summary>
    Task<string> GenerateHLSAsync(string inputFilePath, string outputDirectory, List<QualityPreset> presets, IProgress<int>? progress = null);

    /// <summary>
    /// Generate video thumbnails at different timestamps
    /// </summary>
    Task<List<string>> GenerateThumbnailsAsync(string inputFilePath, string outputDirectory, int count);
}
