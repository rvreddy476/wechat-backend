using MediaService.Api.Models;
using Shared.Domain.Common;

namespace MediaService.Api.Services;

public interface IStorageService
{
    Task<Result<string>> UploadFileAsync(Stream fileStream, string filename, string contentType);
    Task<Result<Stream>> DownloadFileAsync(string storagePath);
    Task<Result<bool>> DeleteFileAsync(string storagePath);
    Task<Result<string>> GetFileUrlAsync(string storagePath, int expirationMinutes = 60);
    Task<Result<string>> GetPublicUrlAsync(string storagePath);
    Task<Result<bool>> CopyFileAsync(string sourceStoragePath, string destinationStoragePath);
}
