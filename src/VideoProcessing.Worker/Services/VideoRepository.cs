using MongoDB.Bson;
using MongoDB.Driver;
using VideoProcessing.Worker.Models;

namespace VideoProcessing.Worker.Services;

/// <summary>
/// MongoDB repository for video updates
/// </summary>
public class VideoRepository : IVideoRepository
{
    private readonly IMongoCollection<BsonDocument> _videosCollection;
    private readonly ILogger<VideoRepository> _logger;

    public VideoRepository(ILogger<VideoRepository> logger, IConfiguration configuration)
    {
        _logger = logger;

        var connectionString = configuration["MongoDB:ConnectionString"]
            ?? throw new ArgumentNullException("MongoDB:ConnectionString configuration is required");
        var databaseName = configuration["MongoDB:DatabaseName"] ?? "wechat_videos";

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        _videosCollection = database.GetCollection<BsonDocument>("videos");

        _logger.LogInformation("VideoRepository initialized with database: {DatabaseName}", databaseName);
    }

    public async Task UpdateProcessingStatusAsync(string videoId, string status, int progress, string? error = null)
    {
        try
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(videoId));

            var update = Builders<BsonDocument>.Update
                .Set("processingStatus", status)
                .Set("processingProgress", progress)
                .Set("updatedAt", DateTime.UtcNow);

            if (!string.IsNullOrEmpty(error))
            {
                update = update.Set("processingError", error);
            }

            await _videosCollection.UpdateOneAsync(filter, update);

            _logger.LogInformation("Updated video {VideoId} status to {Status} ({Progress}%)",
                videoId, status, progress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating processing status for video {VideoId}", videoId);
            throw;
        }
    }

    public async Task UpdateVideoMetadataAsync(string videoId, VideoProcessingResult result)
    {
        try
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(videoId));

            var update = Builders<BsonDocument>.Update
                .Set("duration", result.Metadata!.DurationSeconds)
                .Set("resolution.width", result.Metadata.Width)
                .Set("resolution.height", result.Metadata.Height)
                .Set("resolution.aspectRatio", result.Metadata.AspectRatio)
                .Set("metadata.codec", result.Metadata.VideoCodec)
                .Set("metadata.bitrate", result.Metadata.VideoBitrate)
                .Set("metadata.frameRate", result.Metadata.FrameRate)
                .Set("metadata.audioCodec", result.Metadata.AudioCodec)
                .Set("metadata.audioBitrate", result.Metadata.AudioBitrate)
                .Set("metadata.audioChannels", result.Metadata.AudioChannels)
                .Set("fileSize", result.Metadata.FileSize)
                .Set("updatedAt", DateTime.UtcNow);

            await _videosCollection.UpdateOneAsync(filter, update);

            _logger.LogInformation("Updated metadata for video {VideoId}", videoId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating metadata for video {VideoId}", videoId);
            throw;
        }
    }

    public async Task MarkAsReadyAsync(string videoId, VideoProcessingResult result)
    {
        try
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(videoId));

            // Build quality variants array
            var qualityVariants = result.QualityVariants.Select(qv => new BsonDocument
            {
                { "quality", qv.Quality },
                { "url", qv.Url },
                { "fileSize", qv.FileSize },
                { "bitrate", qv.Bitrate },
                { "codec", qv.Codec }
            }).ToList();

            var update = Builders<BsonDocument>.Update
                .Set("processingStatus", "Ready")
                .Set("processingProgress", 100)
                .Set("processingError", BsonNull.Value)
                .Set("streamingUrl", result.StreamingUrl)
                .Set("thumbnailUrls", new BsonArray(result.ThumbnailUrls))
                .Set("qualityVariants", new BsonArray(qualityVariants))
                .Set("updatedAt", DateTime.UtcNow);

            await _videosCollection.UpdateOneAsync(filter, update);

            _logger.LogInformation("Marked video {VideoId} as Ready with {QualityCount} quality variants and {ThumbnailCount} thumbnails",
                videoId, result.QualityVariants.Count, result.ThumbnailUrls.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking video {VideoId} as ready", videoId);
            throw;
        }
    }

    public async Task MarkAsFailedAsync(string videoId, string errorMessage)
    {
        try
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(videoId));

            var update = Builders<BsonDocument>.Update
                .Set("processingStatus", "Failed")
                .Set("processingError", errorMessage)
                .Set("updatedAt", DateTime.UtcNow);

            await _videosCollection.UpdateOneAsync(filter, update);

            _logger.LogWarning("Marked video {VideoId} as Failed: {Error}", videoId, errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking video {VideoId} as failed", videoId);
            throw;
        }
    }
}
