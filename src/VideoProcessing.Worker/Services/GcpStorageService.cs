using Google.Cloud.Storage.V1;

namespace VideoProcessing.Worker.Services;

/// <summary>
/// Google Cloud Storage implementation for video storage
/// </summary>
public class GcpStorageService : IStorageService
{
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;
    private readonly ILogger<GcpStorageService> _logger;

    public GcpStorageService(ILogger<GcpStorageService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _bucketName = configuration["GCP:Storage:BucketName"]
            ?? throw new ArgumentNullException("GCP:Storage:BucketName configuration is required");

        var credentialsPath = configuration["GCP:CredentialsPath"];

        if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialsPath);
        }

        _storageClient = StorageClient.Create();
        _logger.LogInformation("GCP Storage client initialized for bucket: {BucketName}", _bucketName);
    }

    public async Task<string> UploadFileAsync(string filePath, string destinationPath, string contentType = "video/mp4")
    {
        try
        {
            _logger.LogInformation("Uploading {FilePath} to {Destination}", filePath, destinationPath);

            using var fileStream = File.OpenRead(filePath);

            var uploadedObject = await _storageClient.UploadObjectAsync(
                bucket: _bucketName,
                objectName: destinationPath,
                contentType: contentType,
                source: fileStream
            );

            var publicUrl = GetPublicUrl(destinationPath);

            _logger.LogInformation("Successfully uploaded {FilePath} to {Url}", filePath, publicUrl);

            return publicUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading {FilePath} to GCS", filePath);
            throw;
        }
    }

    public async Task<string> UploadDirectoryAsync(string directoryPath, string destinationPrefix)
    {
        try
        {
            _logger.LogInformation("Uploading directory {DirectoryPath} to {Prefix}", directoryPath, destinationPrefix);

            var files = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            var uploadTasks = new List<Task>();

            foreach (var filePath in files)
            {
                var relativePath = Path.GetRelativePath(directoryPath, filePath);
                var destinationPath = $"{destinationPrefix}/{relativePath}".Replace("\\", "/");

                // Determine content type based on extension
                var extension = Path.GetExtension(filePath).ToLowerInvariant();
                var contentType = extension switch
                {
                    ".m3u8" => "application/vnd.apple.mpegurl",
                    ".ts" => "video/mp2t",
                    ".mp4" => "video/mp4",
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    _ => "application/octet-stream"
                };

                uploadTasks.Add(UploadFileAsync(filePath, destinationPath, contentType));
            }

            await Task.WhenAll(uploadTasks);

            var masterPlaylistUrl = GetPublicUrl($"{destinationPrefix}/master.m3u8");

            _logger.LogInformation("Successfully uploaded directory {DirectoryPath}", directoryPath);

            return masterPlaylistUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading directory {DirectoryPath}", directoryPath);
            throw;
        }
    }

    public async Task DeleteFileAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Deleting {FilePath} from GCS", filePath);

            await _storageClient.DeleteObjectAsync(_bucketName, filePath);

            _logger.LogInformation("Successfully deleted {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {FilePath} from GCS", filePath);
            throw;
        }
    }

    public string GetPublicUrl(string filePath)
    {
        // GCS public URL format
        return $"https://storage.googleapis.com/{_bucketName}/{filePath}";
    }
}
