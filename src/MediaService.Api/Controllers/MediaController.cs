using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MediaService.Api.Models;
using MediaService.Api.Repositories;
using MediaService.Api.Services;
using Shared.Contracts.Common;

namespace MediaService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MediaController : ControllerBase
{
    private readonly IMediaRepository _repository;
    private readonly IStorageService _storageService;
    private readonly IMediaProcessingService _processingService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MediaController> _logger;

    public MediaController(
        IMediaRepository repository,
        IStorageService storageService,
        IMediaProcessingService processingService,
        IConfiguration configuration,
        ILogger<MediaController> logger)
    {
        _repository = repository;
        _storageService = storageService;
        _processingService = processingService;
        _configuration = configuration;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    [HttpPost("upload")]
    [RequestSizeLimit(104857600)] // 100MB
    public async Task<ActionResult<ApiResponse<Media>>> UploadMedia(
        [FromForm] IFormFile file,
        [FromForm] MediaType mediaType,
        [FromForm] string? entityType = null,
        [FromForm] string? entityId = null,
        [FromForm] string? altText = null,
        [FromForm] bool isPublic = false,
        [FromForm] bool generateThumbnail = true)
    {
        var userId = GetCurrentUserId();

        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse<Media>.ErrorResponse("No file uploaded"));
        }

        // Validate file size
        var maxFileSize = long.Parse(_configuration["MediaSettings:MaxFileSize"] ?? "104857600");
        if (file.Length > maxFileSize)
        {
            return BadRequest(ApiResponse<Media>.ErrorResponse($"File size exceeds maximum allowed ({maxFileSize / 1024 / 1024}MB)"));
        }

        // Validate file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = GetAllowedExtensions(mediaType);

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(ApiResponse<Media>.ErrorResponse($"File type '{extension}' is not allowed for {mediaType}"));
        }

        try
        {
            using var stream = file.OpenReadStream();

            // Upload original file
            var uploadResult = await _storageService.UploadFileAsync(stream, file.FileName, file.ContentType);
            if (!uploadResult.IsSuccess)
            {
                return BadRequest(ApiResponse<Media>.ErrorResponse(uploadResult.Error));
            }

            var storagePath = uploadResult.Value;
            var urlResult = await _storageService.GetPublicUrlAsync(storagePath);

            // Get dimensions for images
            int? width = null;
            int? height = null;
            string? thumbnailUrl = null;

            if (mediaType == MediaType.Image)
            {
                stream.Position = 0;
                var dimensionsResult = await _processingService.GetImageDimensionsAsync(stream);
                if (dimensionsResult.IsSuccess)
                {
                    width = dimensionsResult.Value.Width;
                    height = dimensionsResult.Value.Height;
                }

                // Generate thumbnail if requested
                if (generateThumbnail)
                {
                    stream.Position = 0;
                    var thumbnailWidth = int.Parse(_configuration["MediaSettings:ThumbnailWidth"] ?? "300");
                    var thumbnailHeight = int.Parse(_configuration["MediaSettings:ThumbnailHeight"] ?? "300");

                    var thumbnailResult = await _processingService.GenerateThumbnailAsync(
                        stream, thumbnailWidth, thumbnailHeight, maintainAspectRatio: true);

                    if (thumbnailResult.IsSuccess)
                    {
                        var thumbnailFilename = $"thumb_{Path.GetFileNameWithoutExtension(file.FileName)}.jpg";
                        var thumbnailUploadResult = await _storageService.UploadFileAsync(
                            thumbnailResult.Value, thumbnailFilename, "image/jpeg");

                        if (thumbnailUploadResult.IsSuccess)
                        {
                            var thumbnailPathResult = await _storageService.GetPublicUrlAsync(thumbnailUploadResult.Value);
                            if (thumbnailPathResult.IsSuccess)
                            {
                                thumbnailUrl = thumbnailPathResult.Value;
                            }
                        }
                    }
                }
            }

            // Extract metadata
            stream.Position = 0;
            var metadataResult = await _processingService.ExtractMediaMetadataAsync(stream, mediaType);
            var metadata = metadataResult.IsSuccess ? metadataResult.Value : new MediaMetadata();

            // Create media record
            var media = new Media
            {
                UserId = userId,
                Filename = Path.GetFileName(storagePath),
                OriginalFilename = file.FileName,
                ContentType = file.ContentType,
                MediaType = mediaType,
                FileSize = file.Length,
                Url = urlResult.Value,
                ThumbnailUrl = thumbnailUrl,
                Width = width,
                Height = height,
                Metadata = metadata,
                StorageProvider = StorageProvider.Local,
                StoragePath = storagePath,
                Status = MediaStatus.Ready,
                IsPublic = isPublic,
                AltText = altText,
                EntityType = entityType,
                EntityId = entityId,
                ProcessedAt = DateTime.UtcNow
            };

            var createResult = await _repository.CreateMediaAsync(media);

            if (!createResult.IsSuccess)
            {
                // Cleanup uploaded files
                await _storageService.DeleteFileAsync(storagePath);
                return BadRequest(ApiResponse<Media>.ErrorResponse(createResult.Error));
            }

            return Ok(ApiResponse<Media>.SuccessResponse(createResult.Value));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading media for user {UserId}", userId);
            return StatusCode(500, ApiResponse<Media>.ErrorResponse($"Failed to upload media: {ex.Message}"));
        }
    }

    [HttpGet("{mediaId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<Media>>> GetMedia(string mediaId)
    {
        var result = await _repository.GetMediaByIdAsync(mediaId);

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<Media>.ErrorResponse(result.Error));
        }

        var media = result.Value;

        // Check if user has access
        if (!media.IsPublic && User.Identity?.IsAuthenticated == true)
        {
            var userId = GetCurrentUserId();
            if (media.UserId != userId)
            {
                return Forbid();
            }
        }
        else if (!media.IsPublic && User.Identity?.IsAuthenticated != true)
        {
            return Unauthorized();
        }

        return Ok(ApiResponse<Media>.SuccessResponse(media));
    }

    [HttpGet("user")]
    public async Task<ActionResult<ApiResponse<List<Media>>>> GetUserMedia(
        [FromQuery] MediaType? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.GetUserMediaAsync(userId, type, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Media>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Media>>.SuccessResponse(result.Value));
    }

    [HttpPut("{mediaId}")]
    public async Task<ActionResult<ApiResponse<Media>>> UpdateMedia(
        string mediaId,
        [FromBody] UpdateMediaRequest request)
    {
        var userId = GetCurrentUserId();

        var mediaResult = await _repository.GetMediaByIdAsync(mediaId);
        if (!mediaResult.IsSuccess)
        {
            return NotFound(ApiResponse<Media>.ErrorResponse(mediaResult.Error));
        }

        var media = mediaResult.Value;

        if (media.UserId != userId)
        {
            return Forbid();
        }

        // Update fields
        if (request.AltText != null)
            media.AltText = request.AltText;

        if (request.IsPublic.HasValue)
            media.IsPublic = request.IsPublic.Value;

        if (request.Tags != null)
            media.Tags = request.Tags;

        var updateResult = await _repository.UpdateMediaAsync(mediaId, media);

        if (!updateResult.IsSuccess)
        {
            return BadRequest(ApiResponse<Media>.ErrorResponse(updateResult.Error));
        }

        return Ok(ApiResponse<Media>.SuccessResponse(media));
    }

    [HttpDelete("{mediaId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMedia(string mediaId)
    {
        var userId = GetCurrentUserId();

        var mediaResult = await _repository.GetMediaByIdAsync(mediaId);
        if (!mediaResult.IsSuccess)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(mediaResult.Error));
        }

        var media = mediaResult.Value;

        if (media.UserId != userId)
        {
            return Forbid();
        }

        // Delete from storage
        await _storageService.DeleteFileAsync(media.StoragePath);
        if (!string.IsNullOrEmpty(media.ThumbnailUrl))
        {
            // Extract storage path from thumbnail URL and delete
            // This is simplified; in production, you'd store the thumbnail storage path
        }

        // Mark as deleted in database
        var result = await _repository.DeleteMediaAsync(mediaId, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpGet("storage/stats")]
    public async Task<ActionResult<ApiResponse<StorageStatsResponse>>> GetStorageStats()
    {
        var userId = GetCurrentUserId();

        var storageUsedResult = await _repository.GetUserStorageUsedAsync(userId);
        var totalCountResult = await _repository.GetUserMediaCountAsync(userId);
        var imageCountResult = await _repository.GetUserMediaCountAsync(userId, MediaType.Image);
        var videoCountResult = await _repository.GetUserMediaCountAsync(userId, MediaType.Video);

        var stats = new StorageStatsResponse
        {
            StorageUsedBytes = storageUsedResult.IsSuccess ? storageUsedResult.Value : 0,
            TotalFiles = totalCountResult.IsSuccess ? totalCountResult.Value : 0,
            ImageCount = imageCountResult.IsSuccess ? imageCountResult.Value : 0,
            VideoCount = videoCountResult.IsSuccess ? videoCountResult.Value : 0
        };

        return Ok(ApiResponse<StorageStatsResponse>.SuccessResponse(stats));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<Media>>>> SearchMedia(
        [FromQuery] string query,
        [FromQuery] MediaType? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(ApiResponse<List<Media>>.ErrorResponse("Search query is required"));
        }

        var result = await _repository.SearchMediaAsync(query, type, page, pageSize);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Media>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Media>>.SuccessResponse(result.Value));
    }

    [HttpPost("upload/init")]
    public async Task<ActionResult<ApiResponse<Upload>>> InitiateChunkedUpload(
        [FromBody] InitiateUploadRequest request)
    {
        var userId = GetCurrentUserId();

        var upload = new Upload
        {
            UserId = userId,
            UploadKey = Guid.NewGuid().ToString(),
            Filename = request.Filename,
            ContentType = request.ContentType,
            FileSize = request.FileSize,
            ChunkSize = request.ChunkSize,
            TotalChunks = (int)Math.Ceiling((double)request.FileSize / request.ChunkSize)
        };

        var result = await _repository.CreateUploadAsync(upload);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Upload>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<Upload>.SuccessResponse(result.Value));
    }

    private List<string> GetAllowedExtensions(MediaType mediaType)
    {
        return mediaType switch
        {
            MediaType.Image => _configuration.GetSection("MediaSettings:AllowedImageExtensions").Get<List<string>>() ?? new(),
            MediaType.Video => _configuration.GetSection("MediaSettings:AllowedVideoExtensions").Get<List<string>>() ?? new(),
            MediaType.Audio => _configuration.GetSection("MediaSettings:AllowedAudioExtensions").Get<List<string>>() ?? new(),
            MediaType.Document => _configuration.GetSection("MediaSettings:AllowedDocumentExtensions").Get<List<string>>() ?? new(),
            _ => new List<string>()
        };
    }
}

public record UpdateMediaRequest
{
    public string? AltText { get; init; }
    public bool? IsPublic { get; init; }
    public List<string>? Tags { get; init; }
}

public record StorageStatsResponse
{
    public long StorageUsedBytes { get; init; }
    public int TotalFiles { get; init; }
    public int ImageCount { get; init; }
    public int VideoCount { get; init; }
}

public record InitiateUploadRequest
{
    public required string Filename { get; init; }
    public required string ContentType { get; init; }
    public required long FileSize { get; init; }
    public required long ChunkSize { get; init; }
}
