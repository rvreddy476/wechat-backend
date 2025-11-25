using MediaService.Api.Models;
using Shared.Domain.Common;

namespace MediaService.Api.Services;

public class LocalStorageService : IStorageService
{
    private readonly string _storagePath;
    private readonly string _baseUrl;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(IConfiguration configuration, ILogger<LocalStorageService> logger)
    {
        _storagePath = configuration["MediaSettings:LocalStoragePath"] ?? "./uploads";
        _baseUrl = configuration["MediaSettings:CDNBaseUrl"] ?? "";
        _logger = logger;

        // Ensure storage directory exists
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<Result<string>> UploadFileAsync(Stream fileStream, string filename, string contentType)
    {
        try
        {
            // Generate unique filename
            var extension = Path.GetExtension(filename);
            var uniqueFilename = $"{Guid.NewGuid()}{extension}";

            // Create date-based subdirectory
            var dateFolder = DateTime.UtcNow.ToString("yyyy/MM/dd");
            var fullFolderPath = Path.Combine(_storagePath, dateFolder);

            if (!Directory.Exists(fullFolderPath))
            {
                Directory.CreateDirectory(fullFolderPath);
            }

            var fullPath = Path.Combine(fullFolderPath, uniqueFilename);
            var storagePath = Path.Combine(dateFolder, uniqueFilename).Replace("\\", "/");

            // Save file
            using (var fileStreamOut = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(fileStreamOut);
            }

            _logger.LogInformation("Uploaded file to local storage: {StoragePath}", storagePath);
            return Result<string>.Success(storagePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to local storage: {Filename}", filename);
            return Result<string>.Failure($"Failed to upload file: {ex.Message}");
        }
    }

    public async Task<Result<Stream>> DownloadFileAsync(string storagePath)
    {
        try
        {
            var fullPath = Path.Combine(_storagePath, storagePath);

            if (!File.Exists(fullPath))
            {
                return Result<Stream>.Failure("File not found");
            }

            var memoryStream = new MemoryStream();
            using (var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
            {
                await fileStream.CopyToAsync(memoryStream);
            }

            memoryStream.Position = 0;
            return Result<Stream>.Success(memoryStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from local storage: {StoragePath}", storagePath);
            return Result<Stream>.Failure($"Failed to download file: {ex.Message}");
        }
    }

    public Task<Result<bool>> DeleteFileAsync(string storagePath)
    {
        try
        {
            var fullPath = Path.Combine(_storagePath, storagePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Deleted file from local storage: {StoragePath}", storagePath);
            }

            return Task.FromResult(Result<bool>.Success(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from local storage: {StoragePath}", storagePath);
            return Task.FromResult(Result<bool>.Failure($"Failed to delete file: {ex.Message}"));
        }
    }

    public Task<Result<string>> GetFileUrlAsync(string storagePath, int expirationMinutes = 60)
    {
        // For local storage, we don't support expiring URLs
        // Return the public URL instead
        return GetPublicUrlAsync(storagePath);
    }

    public Task<Result<string>> GetPublicUrlAsync(string storagePath)
    {
        try
        {
            // If CDN base URL is configured, use it
            if (!string.IsNullOrEmpty(_baseUrl))
            {
                var url = $"{_baseUrl.TrimEnd('/')}/{storagePath}";
                return Task.FromResult(Result<string>.Success(url));
            }

            // Otherwise return relative path
            var relativeUrl = $"/media/{storagePath}";
            return Task.FromResult(Result<string>.Success(relativeUrl));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting public URL: {StoragePath}", storagePath);
            return Task.FromResult(Result<string>.Failure($"Failed to get public URL: {ex.Message}"));
        }
    }

    public async Task<Result<bool>> CopyFileAsync(string sourceStoragePath, string destinationStoragePath)
    {
        try
        {
            var sourcePath = Path.Combine(_storagePath, sourceStoragePath);
            var destPath = Path.Combine(_storagePath, destinationStoragePath);

            if (!File.Exists(sourcePath))
            {
                return Result<bool>.Failure("Source file not found");
            }

            // Ensure destination directory exists
            var destDirectory = Path.GetDirectoryName(destPath);
            if (destDirectory != null && !Directory.Exists(destDirectory))
            {
                Directory.CreateDirectory(destDirectory);
            }

            File.Copy(sourcePath, destPath, overwrite: true);
            await Task.CompletedTask;

            _logger.LogInformation("Copied file from {Source} to {Destination}", sourceStoragePath, destinationStoragePath);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error copying file from {Source} to {Destination}", sourceStoragePath, destinationStoragePath);
            return Result<bool>.Failure($"Failed to copy file: {ex.Message}");
        }
    }
}
