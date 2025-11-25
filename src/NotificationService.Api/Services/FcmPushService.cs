using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using NotificationService.Api.Models;
using NotificationService.Api.Repositories;
using Shared.Domain.Common;

namespace NotificationService.Api.Services;

public class FcmPushService : IPushService
{
    private readonly INotificationRepository _repository;
    private readonly ILogger<FcmPushService> _logger;
    private readonly bool _isEnabled;

    public FcmPushService(
        INotificationRepository repository,
        IConfiguration configuration,
        ILogger<FcmPushService> logger)
    {
        _repository = repository;
        _logger = logger;
        _isEnabled = bool.Parse(configuration["NotificationSettings:EnablePushNotifications"] ?? "false");

        if (_isEnabled)
        {
            var credentialsPath = configuration["NotificationSettings:FirebaseCredentialsPath"];
            if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
            {
                try
                {
                    // Check if Firebase app already exists
                    if (FirebaseApp.DefaultInstance == null)
                    {
                        FirebaseApp.Create(new AppOptions
                        {
                            Credential = GoogleCredential.FromFile(credentialsPath)
                        });
                    }
                    _logger.LogInformation("Firebase Admin SDK initialized successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize Firebase Admin SDK");
                    _isEnabled = false;
                }
            }
            else
            {
                _logger.LogWarning("Firebase credentials not found at {Path}. Push notifications disabled.", credentialsPath);
                _isEnabled = false;
            }
        }
    }

    public async Task<Result<bool>> SendPushNotificationAsync(Notification notification)
    {
        if (!_isEnabled)
        {
            _logger.LogInformation("Push notifications disabled, skipping for notification {NotificationId}", notification.Id);
            return Result<bool>.Success(false);
        }

        try
        {
            // Get user's device tokens
            var tokensResult = await _repository.GetUserDeviceTokensAsync(
                notification.RecipientId,
                activeOnly: true);

            if (!tokensResult.IsSuccess || !tokensResult.Value.Any())
            {
                _logger.LogWarning("No device tokens found for user {UserId}", notification.RecipientId);
                return Result<bool>.Success(false);
            }

            var deviceTokens = tokensResult.Value;

            // Build FCM messages
            var messages = new List<Message>();
            foreach (var deviceToken in deviceTokens)
            {
                var message = await BuildFcmMessageAsync(notification, deviceToken);
                messages.Add(message);
            }

            // Send in batch (FCM supports up to 500 per batch)
            if (messages.Count == 1)
            {
                var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(messages[0]);
                _logger.LogInformation("Sent FCM notification {MessageId} to user {UserId}",
                    messageId, notification.RecipientId);
            }
            else
            {
                var response = await FirebaseMessaging.DefaultInstance.SendAllAsync(messages);
                _logger.LogInformation("Sent {Success}/{Total} FCM notifications to user {UserId}",
                    response.SuccessCount, messages.Count, notification.RecipientId);

                // Handle failures - mark tokens as inactive
                for (int i = 0; i < response.Responses.Count; i++)
                {
                    if (!response.Responses[i].IsSuccess)
                    {
                        var error = response.Responses[i].Exception;
                        if (IsTokenInvalid(error))
                        {
                            await _repository.UnregisterDeviceTokenAsync(
                                deviceTokens[i].Id,
                                notification.RecipientId);

                            _logger.LogWarning("Removed invalid token for user {UserId}: {Error}",
                                notification.RecipientId, error?.Message);
                        }
                    }
                }
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending FCM push notification to user {UserId}",
                notification.RecipientId);
            return Result<bool>.Failure($"FCM push failed: {ex.Message}");
        }
    }

    public async Task<Result<string>> SendToTopicAsync(
        string topic,
        string title,
        string body,
        NotificationType type,
        Dictionary<string, string>? data = null)
    {
        if (!_isEnabled)
        {
            _logger.LogInformation("Push notifications disabled, skipping topic message");
            return Result<string>.Failure("Push notifications disabled");
        }

        try
        {
            var message = new Message
            {
                Topic = topic,
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data ?? new Dictionary<string, string>(),
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ChannelId = GetChannelId(type),
                        Sound = "default"
                    }
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps
                    {
                        Sound = "default",
                        ContentAvailable = true
                    }
                }
            };

            var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("Sent FCM topic message {MessageId} to topic {Topic}", messageId, topic);

            return Result<string>.Success(messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending FCM topic message to {Topic}", topic);
            return Result<string>.Failure($"Topic send failed: {ex.Message}");
        }
    }

    public async Task<Result<bool>> SubscribeToTopicAsync(List<string> deviceTokens, string topic)
    {
        if (!_isEnabled)
        {
            return Result<bool>.Failure("Push notifications disabled");
        }

        try
        {
            var response = await FirebaseMessaging.DefaultInstance
                .SubscribeToTopicAsync(deviceTokens, topic);

            _logger.LogInformation(
                "Subscribed {Success}/{Total} devices to topic {Topic}",
                response.SuccessCount,
                deviceTokens.Count,
                topic);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to topic {Topic}", topic);
            return Result<bool>.Failure($"Subscribe error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UnsubscribeFromTopicAsync(List<string> deviceTokens, string topic)
    {
        if (!_isEnabled)
        {
            return Result<bool>.Failure("Push notifications disabled");
        }

        try
        {
            var response = await FirebaseMessaging.DefaultInstance
                .UnsubscribeFromTopicAsync(deviceTokens, topic);

            _logger.LogInformation(
                "Unsubscribed {Success}/{Total} devices from topic {Topic}",
                response.SuccessCount,
                deviceTokens.Count,
                topic);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from topic {Topic}", topic);
            return Result<bool>.Failure($"Unsubscribe error: {ex.Message}");
        }
    }

    private async Task<Message> BuildFcmMessageAsync(Notification notification, DeviceToken deviceToken)
    {
        var message = new Message
        {
            Token = deviceToken.Token,
            Notification = new FirebaseAdmin.Messaging.Notification
            {
                Title = notification.Title,
                Body = notification.Message,
                ImageUrl = notification.ImageUrl
            },
            Data = new Dictionary<string, string>
            {
                { "notificationId", notification.Id },
                { "type", notification.Type.ToString() },
                { "entityId", notification.EntityId ?? "" },
                { "entityType", notification.EntityType?.ToString() ?? "" },
                { "actionUrl", notification.ActionUrl ?? "" },
                { "senderId", notification.SenderId?.ToString() ?? "" },
                { "senderUsername", notification.SenderUsername ?? "" },
                { "timestamp", notification.CreatedAt.ToString("O") }
            }
        };

        // Platform-specific configuration
        if (deviceToken.Platform == DevicePlatform.Android)
        {
            message.Android = new AndroidConfig
            {
                Priority = notification.Priority == NotificationPriority.High
                    ? Priority.High
                    : Priority.Normal,
                Notification = new AndroidNotification
                {
                    ChannelId = GetChannelId(notification.Type),
                    Sound = "default",
                    Color = "#007AFF",
                    Tag = notification.EntityId, // Groups notifications
                    ClickAction = "FLUTTER_NOTIFICATION_CLICK",
                    Icon = "ic_notification",
                    DefaultSound = true,
                    DefaultVibrateTimings = true
                },
                TimeToLive = TimeSpan.FromDays(7) // Message expiration
            };
        }
        else if (deviceToken.Platform == DevicePlatform.iOS)
        {
            var unreadCount = await GetUserUnreadCountAsync(notification.RecipientId);

            message.Apns = new ApnsConfig
            {
                Headers = new Dictionary<string, string>
                {
                    { "apns-priority", notification.Priority == NotificationPriority.High ? "10" : "5" },
                    { "apns-expiration", DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds().ToString() }
                },
                Aps = new Aps
                {
                    Alert = new ApsAlert
                    {
                        Title = notification.Title,
                        Body = notification.Message,
                        LaunchImage = notification.ImageUrl
                    },
                    Sound = "default",
                    Badge = unreadCount,
                    ContentAvailable = true,
                    MutableContent = true,
                    Category = notification.Type.ToString(),
                    ThreadId = notification.EntityId ?? notification.Id
                }
            };
        }
        else if (deviceToken.Platform == DevicePlatform.Web)
        {
            message.Webpush = new WebpushConfig
            {
                Headers = new Dictionary<string, string>
                {
                    { "TTL", "604800" } // 7 days in seconds
                },
                Notification = new WebpushNotification
                {
                    Title = notification.Title,
                    Body = notification.Message,
                    Icon = notification.SenderAvatarUrl ?? "/icon-192x192.png",
                    Image = notification.ImageUrl,
                    Badge = "/badge-72x72.png",
                    Tag = notification.EntityId ?? notification.Id,
                    RequireInteraction = notification.Priority == NotificationPriority.High,
                    Silent = false,
                    Vibrate = new[] { 200, 100, 200 },
                    Timestamp = notification.CreatedAt.Ticks
                },
                FcmOptions = new WebpushFcmOptions
                {
                    Link = notification.ActionUrl ?? "/"
                }
            };
        }

        return message;
    }

    private string GetChannelId(NotificationType type)
    {
        return type switch
        {
            NotificationType.NewMessage or
            NotificationType.MessageReaction or
            NotificationType.GroupMessageMention => "messages",

            NotificationType.Like or
            NotificationType.Comment or
            NotificationType.Reply or
            NotificationType.Mention => "interactions",

            NotificationType.Follow or
            NotificationType.FollowRequestAccepted or
            NotificationType.FollowRequestReceived => "social",

            NotificationType.VideoLike or
            NotificationType.VideoComment or
            NotificationType.NewVideoFromSubscription => "videos",

            NotificationType.PostComment or
            NotificationType.PostLike or
            NotificationType.PostShare => "posts",

            NotificationType.SystemAnnouncement or
            NotificationType.SecurityAlert => "system",

            _ => "default"
        };
    }

    private async Task<int> GetUserUnreadCountAsync(Guid userId)
    {
        var result = await _repository.GetUnreadCountAsync(userId);
        return result.IsSuccess ? result.Value : 0;
    }

    private bool IsTokenInvalid(Exception? exception)
    {
        if (exception is FirebaseMessagingException fcmEx)
        {
            return fcmEx.MessagingErrorCode == MessagingErrorCode.InvalidArgument ||
                   fcmEx.MessagingErrorCode == MessagingErrorCode.Unregistered ||
                   fcmEx.MessagingErrorCode == MessagingErrorCode.SenderIdMismatch;
        }
        return false;
    }
}

public interface IPushService
{
    Task<Result<bool>> SendPushNotificationAsync(Notification notification);
    Task<Result<string>> SendToTopicAsync(string topic, string title, string body, NotificationType type, Dictionary<string, string>? data = null);
    Task<Result<bool>> SubscribeToTopicAsync(List<string> deviceTokens, string topic);
    Task<Result<bool>> UnsubscribeFromTopicAsync(List<string> deviceTokens, string topic);
}
