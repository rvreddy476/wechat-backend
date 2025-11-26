namespace VideoProcessing.Worker.Services;

/// <summary>
/// Service for uploading processed videos to cloud storage
/// </summary>
public interface IStorageService
{
    /// <summary>
    /// Upload a file to cloud storage
    /// </summary>
    Task<string> UploadFileAsync(string filePath, string destinationPath, string contentType = "video/mp4");

    /// <summary>
    /// Upload directory (for HLS segments)
    /// </summary>
    Task<string> UploadDirectoryAsync(string directoryPath, string destinationPrefix);

    /// <summary>
    /// Delete a file from cloud storage
    /// </summary>
    Task DeleteFileAsync(string filePath);

    /// <summary>
    /// Get public URL for a file
    /// </summary>
    string GetPublicUrl(string filePath);
}
