using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using NotificationService.Api.Models;
using NotificationService.Api.Repositories;
using NotificationService.Api.Services;
using Shared.Contracts.Common;

namespace NotificationService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationRepository _repository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationRepository repository,
        INotificationService notificationService,
        ILogger<NotificationsController> logger)
    {
        _repository = repository;
        _notificationService = notificationService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<Notification>>>> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool unreadOnly = false,
        [FromQuery] NotificationType? type = null)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.GetUserNotificationsAsync(userId, page, pageSize, unreadOnly, type);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Notification>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Notification>>.SuccessResponse(result.Value));
    }

    [HttpGet("{notificationId}")]
    public async Task<ActionResult<ApiResponse<Notification>>> GetNotification(string notificationId)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.GetNotificationByIdAsync(notificationId);

        if (!result.IsSuccess)
        {
            return NotFound(ApiResponse<Notification>.ErrorResponse(result.Error));
        }

        // Verify the notification belongs to the current user
        if (result.Value.RecipientId != userId)
        {
            return Forbid();
        }

        return Ok(ApiResponse<Notification>.SuccessResponse(result.Value));
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var result = await _repository.GetUnreadCountAsync(userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<int>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<int>.SuccessResponse(result.Value));
    }

    [HttpPut("{notificationId}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(string notificationId)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.MarkAsReadAsync(notificationId, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpPut("read-all")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAllAsRead()
    {
        var userId = GetCurrentUserId();
        var result = await _repository.MarkAllAsReadAsync(userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpDelete("{notificationId}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteNotification(string notificationId)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.DeleteNotificationAsync(notificationId, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    [HttpDelete]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAllNotifications()
    {
        var userId = GetCurrentUserId();
        var result = await _repository.DeleteAllNotificationsAsync(userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    // Notification Preferences
    [HttpGet("preferences")]
    public async Task<ActionResult<ApiResponse<NotificationPreferences>>> GetPreferences()
    {
        var userId = GetCurrentUserId();
        var result = await _repository.GetUserPreferencesAsync(userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<NotificationPreferences>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<NotificationPreferences>.SuccessResponse(result.Value));
    }

    [HttpPut("preferences")]
    public async Task<ActionResult<ApiResponse<NotificationPreferences>>> UpdatePreferences(
        [FromBody] UpdatePreferencesRequest request)
    {
        var userId = GetCurrentUserId();

        var preferencesResult = await _repository.GetUserPreferencesAsync(userId);
        if (!preferencesResult.IsSuccess)
        {
            return BadRequest(ApiResponse<NotificationPreferences>.ErrorResponse(preferencesResult.Error));
        }

        var preferences = preferencesResult.Value;

        // Update preferences
        if (request.EnableInApp.HasValue)
            preferences.EnableInApp = request.EnableInApp.Value;

        if (request.EnablePush.HasValue)
            preferences.EnablePush = request.EnablePush.Value;

        if (request.EnableEmail.HasValue)
            preferences.EnableEmail = request.EnableEmail.Value;

        if (request.EnableSMS.HasValue)
            preferences.EnableSMS = request.EnableSMS.Value;

        if (request.MuteUntil.HasValue)
            preferences.MuteUntil = request.MuteUntil.Value;

        if (request.QuietHoursStart.HasValue)
            preferences.QuietHoursStart = request.QuietHoursStart.Value;

        if (request.QuietHoursEnd.HasValue)
            preferences.QuietHoursEnd = request.QuietHoursEnd.Value;

        if (request.NotificationTypes != null)
        {
            foreach (var notificationType in request.NotificationTypes)
            {
                preferences.NotificationTypes[notificationType.Key] = notificationType.Value;
            }
        }

        var result = await _repository.CreateOrUpdatePreferencesAsync(preferences);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<NotificationPreferences>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<NotificationPreferences>.SuccessResponse(result.Value));
    }

    // Device Token Management
    [HttpPost("device-tokens")]
    public async Task<ActionResult<ApiResponse<DeviceToken>>> RegisterDeviceToken(
        [FromBody] RegisterDeviceTokenRequest request)
    {
        var userId = GetCurrentUserId();

        var deviceToken = new DeviceToken
        {
            UserId = userId,
            Token = request.Token,
            Platform = request.Platform,
            DeviceId = request.DeviceId,
            DeviceName = request.DeviceName,
            AppVersion = request.AppVersion
        };

        var result = await _repository.RegisterDeviceTokenAsync(deviceToken);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<DeviceToken>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<DeviceToken>.SuccessResponse(result.Value));
    }

    [HttpGet("device-tokens")]
    public async Task<ActionResult<ApiResponse<List<DeviceToken>>>> GetDeviceTokens(
        [FromQuery] bool activeOnly = true)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.GetUserDeviceTokensAsync(userId, activeOnly);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<DeviceToken>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<DeviceToken>>.SuccessResponse(result.Value));
    }

    [HttpDelete("device-tokens/{tokenId}")]
    public async Task<ActionResult<ApiResponse<bool>>> UnregisterDeviceToken(string tokenId)
    {
        var userId = GetCurrentUserId();
        var result = await _repository.UnregisterDeviceTokenAsync(tokenId, userId);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }

    // Send notification (typically called by other services via internal API)
    [HttpPost("send")]
    public async Task<ActionResult<ApiResponse<Notification>>> SendNotification(
        [FromBody] SendNotificationRequest request)
    {
        var notification = new Notification
        {
            RecipientId = request.RecipientId,
            SenderId = request.SenderId,
            SenderUsername = request.SenderUsername,
            SenderAvatarUrl = request.SenderAvatarUrl,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            EntityId = request.EntityId,
            EntityType = request.EntityType,
            ActionUrl = request.ActionUrl,
            ImageUrl = request.ImageUrl,
            Data = request.Data,
            DeliveryChannels = request.DeliveryChannels ?? new List<DeliveryChannel> { DeliveryChannel.InApp },
            Priority = request.Priority ?? NotificationPriority.Normal,
            ExpiresAt = request.ExpiresAt
        };

        var result = await _notificationService.SendNotificationAsync(notification);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<Notification>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<Notification>.SuccessResponse(result.Value));
    }

    // Bulk send (for system notifications)
    [HttpPost("send-bulk")]
    public async Task<ActionResult<ApiResponse<List<Notification>>>> SendBulkNotifications(
        [FromBody] SendBulkNotificationsRequest request)
    {
        var notifications = request.RecipientIds.Select(recipientId => new Notification
        {
            RecipientId = recipientId,
            Type = request.Type,
            Title = request.Title,
            Message = request.Message,
            EntityType = request.EntityType,
            ActionUrl = request.ActionUrl,
            ImageUrl = request.ImageUrl,
            DeliveryChannels = request.DeliveryChannels ?? new List<DeliveryChannel> { DeliveryChannel.InApp },
            Priority = request.Priority ?? NotificationPriority.Normal,
            ExpiresAt = request.ExpiresAt
        }).ToList();

        var result = await _notificationService.SendBulkNotificationsAsync(notifications);

        if (!result.IsSuccess)
        {
            return BadRequest(ApiResponse<List<Notification>>.ErrorResponse(result.Error));
        }

        return Ok(ApiResponse<List<Notification>>.SuccessResponse(result.Value));
    }
}

public record UpdatePreferencesRequest
{
    public bool? EnableInApp { get; init; }
    public bool? EnablePush { get; init; }
    public bool? EnableEmail { get; init; }
    public bool? EnableSMS { get; init; }
    public DateTime? MuteUntil { get; init; }
    public TimeSpan? QuietHoursStart { get; init; }
    public TimeSpan? QuietHoursEnd { get; init; }
    public Dictionary<NotificationType, NotificationChannelSettings>? NotificationTypes { get; init; }
}

public record RegisterDeviceTokenRequest
{
    public required string Token { get; init; }
    public required DevicePlatform Platform { get; init; }
    public string? DeviceId { get; init; }
    public string? DeviceName { get; init; }
    public string? AppVersion { get; init; }
}

public record SendNotificationRequest
{
    public required Guid RecipientId { get; init; }
    public Guid? SenderId { get; init; }
    public string? SenderUsername { get; init; }
    public string? SenderAvatarUrl { get; init; }
    public required NotificationType Type { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public string? EntityId { get; init; }
    public EntityType? EntityType { get; init; }
    public string? ActionUrl { get; init; }
    public string? ImageUrl { get; init; }
    public Dictionary<string, string>? Data { get; init; }
    public List<DeliveryChannel>? DeliveryChannels { get; init; }
    public NotificationPriority? Priority { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

public record SendBulkNotificationsRequest
{
    public required List<Guid> RecipientIds { get; init; }
    public required NotificationType Type { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public EntityType? EntityType { get; init; }
    public string? ActionUrl { get; init; }
    public string? ImageUrl { get; init; }
    public List<DeliveryChannel>? DeliveryChannels { get; init; }
    public NotificationPriority? Priority { get; init; }
    public DateTime? ExpiresAt { get; init; }
}
