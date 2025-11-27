using Microsoft.AspNetCore.SignalR;
using NotificationService.Api.Hubs;
using NotificationService.Api.Models;
using NotificationService.Api.Repositories;
using Shared.Domain.Common;

namespace NotificationService.Api.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IPushService _pushService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repository,
        IHubContext<NotificationHub> hubContext,
        IPushService pushService,
        ILogger<NotificationService> logger)
    {
        _repository = repository;
        _hubContext = hubContext;
        _pushService = pushService;
        _logger = logger;
    }

    public async Task<Result<Notification>> SendNotificationAsync(Notification notification)
    {
        try
        {
            // Create notification in database
            var createResult = await _repository.CreateNotificationAsync(notification);
            if (!createResult.IsSuccess)
            {
                return createResult;
            }

            notification = createResult.Value;

            // Send through enabled channels based on user preferences
            var deliveryTasks = new List<Task<Result<bool>>>();

            foreach (var channel in notification.DeliveryChannels)
            {
                var isEnabledResult = await _repository.IsNotificationEnabledAsync(
                    notification.RecipientId,
                    notification.Type,
                    channel);

                if (isEnabledResult.IsSuccess && isEnabledResult.Value)
                {
                    deliveryTasks.Add(SendToChannelAsync(notification, channel));
                }
            }

            // Send to all channels in parallel
            var deliveryResults = await Task.WhenAll(deliveryTasks);

            // Update delivery status based on results
            var anySuccess = deliveryResults.Any(r => r.IsSuccess && r.Value);
            var allFailed = deliveryResults.All(r => !r.IsSuccess || !r.Value);

            if (allFailed && deliveryResults.Length > 0)
            {
                await _repository.UpdateDeliveryStatusAsync(notification.Id, DeliveryStatus.Failed);
            }
            else if (anySuccess)
            {
                await _repository.UpdateDeliveryStatusAsync(notification.Id, DeliveryStatus.Delivered);
            }

            return Result<Notification>.Success(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}", notification.RecipientId);
            return Result.Failure<Notification>($"Failed to send notification: {ex.Message}");
        }
    }

    public async Task<Result<List<Notification>>> SendBulkNotificationsAsync(List<Notification> notifications)
    {
        try
        {
            // Create all notifications in database
            var createResult = await _repository.CreateBulkNotificationsAsync(notifications);
            if (!createResult.IsSuccess)
            {
                return createResult;
            }

            notifications = createResult.Value;

            // Send notifications in parallel
            var sendTasks = notifications.Select(n => SendNotificationChannelsAsync(n));
            await Task.WhenAll(sendTasks);

            return Result<List<Notification>>.Success(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk notifications");
            return Result.Failure<List<Notification>>($"Failed to send bulk notifications: {ex.Message}");
        }
    }

    private async Task SendNotificationChannelsAsync(Notification notification)
    {
        try
        {
            var deliveryTasks = new List<Task<Result<bool>>>();

            foreach (var channel in notification.DeliveryChannels)
            {
                var isEnabledResult = await _repository.IsNotificationEnabledAsync(
                    notification.RecipientId,
                    notification.Type,
                    channel);

                if (isEnabledResult.IsSuccess && isEnabledResult.Value)
                {
                    deliveryTasks.Add(SendToChannelAsync(notification, channel));
                }
            }

            var deliveryResults = await Task.WhenAll(deliveryTasks);

            var anySuccess = deliveryResults.Any(r => r.IsSuccess && r.Value);
            var allFailed = deliveryResults.All(r => !r.IsSuccess || !r.Value);

            if (allFailed && deliveryResults.Length > 0)
            {
                await _repository.UpdateDeliveryStatusAsync(notification.Id, DeliveryStatus.Failed);
            }
            else if (anySuccess)
            {
                await _repository.UpdateDeliveryStatusAsync(notification.Id, DeliveryStatus.Delivered);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification {NotificationId}", notification.Id);
        }
    }

    public async Task<Result<bool>> SendToChannelAsync(Notification notification, DeliveryChannel channel)
    {
        try
        {
            switch (channel)
            {
                case DeliveryChannel.InApp:
                    return await SendInAppNotificationAsync(notification);

                case DeliveryChannel.Push:
                    return await SendPushNotificationAsync(notification);

                case DeliveryChannel.Email:
                    return await SendEmailNotificationAsync(notification);

                case DeliveryChannel.SMS:
                    return await SendSMSNotificationAsync(notification);

                default:
                    return Result.Failure<bool>($"Unsupported delivery channel: {channel}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification {NotificationId} via {Channel}",
                notification.Id, channel);
            return Result.Failure<bool>($"Failed to send via {channel}: {ex.Message}");
        }
    }

    private async Task<Result<bool>> SendInAppNotificationAsync(Notification notification)
    {
        try
        {
            // Send real-time notification via SignalR
            await _hubContext.Clients
                .User(notification.RecipientId.ToString())
                .SendAsync("ReceiveNotification", new
                {
                    notification.Id,
                    notification.Type,
                    notification.Title,
                    notification.Message,
                    notification.SenderId,
                    notification.SenderUsername,
                    notification.SenderAvatarUrl,
                    notification.EntityId,
                    notification.EntityType,
                    notification.ActionUrl,
                    notification.ImageUrl,
                    notification.Priority,
                    notification.CreatedAt
                });

            _logger.LogInformation("Sent in-app notification {NotificationId} to user {UserId}",
                notification.Id, notification.RecipientId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending in-app notification {NotificationId}", notification.Id);
            return Result.Failure<bool>($"Failed to send in-app notification: {ex.Message}");
        }
    }

    private async Task<Result<bool>> SendPushNotificationAsync(Notification notification)
    {
        try
        {
            // Use FCM push service to send push notifications
            var result = await _pushService.SendPushNotificationAsync(notification);

            if (result.IsSuccess && result.Value)
            {
                _logger.LogInformation("Successfully sent push notification {NotificationId} to user {UserId}",
                    notification.Id, notification.RecipientId);
            }
            else
            {
                _logger.LogWarning("Push notification {NotificationId} not sent to user {UserId}: {Reason}",
                    notification.Id, notification.RecipientId, result.Error ?? "No devices or disabled");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending push notification {NotificationId}", notification.Id);
            return Result.Failure<bool>($"Failed to send push notification: {ex.Message}");
        }
    }

    private async Task<Result<bool>> SendEmailNotificationAsync(Notification notification)
    {
        try
        {
            // TODO: Implement email sending via SMTP or email service (SendGrid, AWS SES, etc.)
            // For now, just log that we would send an email
            _logger.LogInformation("Would send email notification {NotificationId} to user {UserId}",
                notification.Id, notification.RecipientId);

            // In a real implementation, you would:
            // 1. Get user's email address from UserProfileService
            // 2. Build HTML email template
            // 3. Send via SMTP or email service
            // 4. Handle bounces and failures

            await Task.CompletedTask;
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email notification {NotificationId}", notification.Id);
            return Result.Failure<bool>($"Failed to send email notification: {ex.Message}");
        }
    }

    private async Task<Result<bool>> SendSMSNotificationAsync(Notification notification)
    {
        try
        {
            // TODO: Implement SMS sending via Twilio, AWS SNS, etc.
            // For now, just log that we would send an SMS
            _logger.LogInformation("Would send SMS notification {NotificationId} to user {UserId}",
                notification.Id, notification.RecipientId);

            // In a real implementation, you would:
            // 1. Get user's phone number from UserProfileService
            // 2. Format SMS message (keep it short)
            // 3. Send via SMS service
            // 4. Handle delivery failures

            await Task.CompletedTask;
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS notification {NotificationId}", notification.Id);
            return Result.Failure<bool>($"Failed to send SMS notification: {ex.Message}");
        }
    }
}
