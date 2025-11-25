using MediaService.Api.Models;
using Shared.Domain.Common;

namespace MediaService.Api.Services;

public interface IMediaProcessingService
{
    Task<Result<(int Width, int Height)>> GetImageDimensionsAsync(Stream imageStream);
    Task<Result<Stream>> OptimizeImageAsync(Stream imageStream, int quality = 85);
    Task<Result<Stream>> GenerateThumbnailAsync(Stream imageStream, int width, int height, bool maintainAspectRatio = true);
    Task<Result<Stream>> ResizeImageAsync(Stream imageStream, int width, int height);
    Task<Result<MediaMetadata>> ExtractMediaMetadataAsync(Stream fileStream, MediaType mediaType);
}
