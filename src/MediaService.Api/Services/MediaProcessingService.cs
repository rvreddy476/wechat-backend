using MediaService.Api.Models;
using Shared.Domain.Common;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using System.Linq;

namespace MediaService.Api.Services;

public class MediaProcessingService : IMediaProcessingService
{
    private readonly ILogger<MediaProcessingService> _logger;

    public MediaProcessingService(ILogger<MediaProcessingService> logger)
    {
        _logger = logger;
    }

    public async Task<Result<(int Width, int Height)>> GetImageDimensionsAsync(Stream imageStream)
    {
        try
        {
            imageStream.Position = 0;
            var image = await Image.LoadAsync(imageStream);

            return Result<(int Width, int Height)>.Success((image.Width, image.Height));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting image dimensions");
            return Result.Failure<(int Width, int Height)>($"Failed to get image dimensions: {ex.Message}");
        }
    }

    public async Task<Result<Stream>> OptimizeImageAsync(Stream imageStream, int quality = 85)
    {
        try
        {
            imageStream.Position = 0;
            var image = await Image.LoadAsync(imageStream);
            var outputStream = new MemoryStream();

            var encoder = new JpegEncoder
            {
                Quality = quality
            };

            await image.SaveAsync(outputStream, encoder);
            outputStream.Position = 0;

            _logger.LogInformation("Optimized image with quality {Quality}", quality);
            return Result.Success<Stream>(outputStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing image");
            return Result.Failure<Stream>($"Failed to optimize image: {ex.Message}");
        }
    }

    public async Task<Result<Stream>> GenerateThumbnailAsync(Stream imageStream, int width, int height, bool maintainAspectRatio = true)
    {
        try
        {
            imageStream.Position = 0;
            var image = await Image.LoadAsync(imageStream);
            var outputStream = new MemoryStream();

            if (maintainAspectRatio)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(width, height),
                    Mode = ResizeMode.Max
                }));
            }
            else
            {
                image.Mutate(x => x.Resize(width, height));
            }

            var encoder = new JpegEncoder
            {
                Quality = 80
            };

            await image.SaveAsync(outputStream, encoder);
            outputStream.Position = 0;

            _logger.LogInformation("Generated thumbnail {Width}x{Height}", width, height);
            return Result.Success<Stream>(outputStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating thumbnail");
            return Result.Failure<Stream>($"Failed to generate thumbnail: {ex.Message}");
        }
    }

    public async Task<Result<Stream>> ResizeImageAsync(Stream imageStream, int width, int height)
    {
        try
        {
            imageStream.Position = 0;
            var image = await Image.LoadAsync(imageStream);
            var outputStream = new MemoryStream();

            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(width, height),
                Mode = ResizeMode.Max
            }));

            await image.SaveAsync(outputStream, image.Metadata.DecodedImageFormat ?? JpegFormat.Instance);
            outputStream.Position = 0;

            _logger.LogInformation("Resized image to {Width}x{Height}", width, height);
            return Result.Success<Stream>(outputStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resizing image");
            return Result.Failure<Stream>($"Failed to resize image: {ex.Message}");
        }
    }

    public async Task<Result<MediaMetadata>> ExtractMediaMetadataAsync(Stream fileStream, MediaType mediaType)
    {
        try
        {
            var metadata = new MediaMetadata();

            if (mediaType == MediaType.Image)
            {
                fileStream.Position = 0;
                var image = await Image.LoadAsync(fileStream);

                metadata.Format = image.Metadata.DecodedImageFormat?.Name;

                // Extract EXIF data
                if (image.Metadata.ExifProfile != null)
                {
                    metadata.ExifData = new Dictionary<string, string>();

                    foreach (var value in image.Metadata.ExifProfile.Values)
                    {
                        try
                        {
                            var stringValue = value.GetValue()?.ToString();
                            if (!string.IsNullOrEmpty(stringValue))
                            {
                                metadata.ExifData[value.Tag.ToString()] = stringValue;
                            }
                        }
                        catch
                        {
                            // Skip invalid EXIF values
                        }
                    }

                    // Try to extract GPS location
                    var exifValues = image.Metadata.ExifProfile?.Values;
                    if (exifValues != null)
                    {
                        var latEntry = exifValues.FirstOrDefault(v => v.Tag == ExifTag.GPSLatitude);
                        var lonEntry = exifValues.FirstOrDefault(v => v.Tag == ExifTag.GPSLongitude);

                        if (latEntry?.GetValue() is Rational[] latCoords && lonEntry?.GetValue() is Rational[] lonCoords)
                        {
                            try
                            {
                                var latValue = ToDecimalDegrees(latCoords);
                                var lonValue = ToDecimalDegrees(lonCoords);

                                metadata.Location = new MediaLocation
                                {
                                    Latitude = latValue,
                                    Longitude = lonValue
                                };
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to parse GPS coordinates");
                            }
                        }
                    }
                }
            }
            else if (mediaType == MediaType.Video || mediaType == MediaType.Audio)
            {
                // TODO: Implement video/audio metadata extraction using FFmpeg or similar
                // For now, return empty metadata
                _logger.LogInformation("Video/Audio metadata extraction not yet implemented");
            }

            return Result.Success<MediaMetadata>(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting media metadata");
            return Result.Failure<MediaMetadata>($"Failed to extract metadata: {ex.Message}");
        }
    }

    private double ToDecimalDegrees(Rational[] coordinates)
    {
        if (coordinates == null || coordinates.Length != 3)
        {
            throw new ArgumentException("Invalid GPS coordinates");
        }

        var degrees = coordinates[0].ToDouble();
        var minutes = coordinates[1].ToDouble();
        var seconds = coordinates[2].ToDouble();

        return degrees + (minutes / 60) + (seconds / 3600);
    }
}
