using MongoDB.Driver;
using NotificationService.Api.Models;
using Shared.Domain.Common;
using Shared.Infrastructure.MongoDB;

namespace NotificationService.Api.Repositories;

public class NotificationRepository : MongoRepository<Notification>, INotificationRepository
{
    private readonly IMongoCollection<NotificationPreferences> _preferencesCollection;
    private readonly IMongoCollection<DeviceToken> _deviceTokensCollection;
    private readonly ILogger<NotificationRepository> _logger;

    public NotificationRepository(
        IMongoDatabase database,
        ILogger<NotificationRepository> logger) : base(database, "notifications")
    {
        _preferencesCollection = database.GetCollection<NotificationPreferences>("notificationPreferences");
        _deviceTokensCollection = database.GetCollection<DeviceToken>("deviceTokens");
        _logger = logger;
    }

    // Notification Management
    public async Task<Result<Notification>> CreateNotificationAsync(Notification notification)
    {
        try
        {
            notification.CreatedAt = DateTime.UtcNow;
            notification.UpdatedAt = DateTime.UtcNow;

            await Collection.InsertOneAsync(notification);
            _logger.LogInformation("Created notification {NotificationId} for user {UserId}", notification.Id, notification.RecipientId);
            return Result<Notification>.Success(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating notification for user {UserId}", notification.RecipientId);
            return Result.Failure<Notification>($"Failed to create notification: {ex.Message}");
        }
    }

    public async Task<Result<Notification>> GetNotificationByIdAsync(string notificationId)
    {
        try
        {
            var notification = await Collection
                .Find(n => n.Id == notificationId)
                .FirstOrDefaultAsync();

            if (notification == null)
            {
                return Result.Failure<Notification>("Notification not found");
            }

            return Result<Notification>.Success(notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notification {NotificationId}", notificationId);
            return Result.Failure<Notification>($"Failed to get notification: {ex.Message}");
        }
    }

    public async Task<Result<List<Notification>>> GetUserNotificationsAsync(
        Guid userId,
        int page = 1,
        int pageSize = 20,
        bool unreadOnly = false,
        NotificationType? type = null)
    {
        try
        {
            var filterBuilder = Builders<Notification>.Filter;
            var filter = filterBuilder.Eq(n => n.RecipientId, userId);

            if (unreadOnly)
            {
                filter &= filterBuilder.Eq(n => n.IsRead, false);
            }

            if (type.HasValue)
            {
                filter &= filterBuilder.Eq(n => n.Type, type.Value);
            }

            // Filter out expired notifications
            filter &= filterBuilder.Or(
                filterBuilder.Eq(n => n.ExpiresAt, null),
                filterBuilder.Gt(n => n.ExpiresAt, DateTime.UtcNow)
            );

            var notifications = await Collection
                .Find(filter)
                .SortByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Result<List<Notification>>.Success(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications for user {UserId}", userId);
            return Result.Failure<List<Notification>>($"Failed to get notifications: {ex.Message}");
        }
    }

    public async Task<Result<int>> GetUnreadCountAsync(Guid userId)
    {
        try
        {
            var filterBuilder = Builders<Notification>.Filter;
            var filter = filterBuilder.Eq(n => n.RecipientId, userId) &
                         filterBuilder.Eq(n => n.IsRead, false);

            // Filter out expired notifications
            filter &= filterBuilder.Or(
                filterBuilder.Eq(n => n.ExpiresAt, null),
                filterBuilder.Gt(n => n.ExpiresAt, DateTime.UtcNow)
            );

            var count = await Collection.CountDocumentsAsync(filter);
            return Result<int>.Success((int)count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unread count for user {UserId}", userId);
            return Result.Failure<int>($"Failed to get unread count: {ex.Message}");
        }
    }

    public async Task<Result<bool>> MarkAsReadAsync(string notificationId, Guid userId)
    {
        try
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId) &
                         Builders<Notification>.Filter.Eq(n => n.RecipientId, userId);

            var update = Builders<Notification>.Update
                .Set(n => n.IsRead, true)
                .Set(n => n.ReadAt, DateTime.UtcNow)
                .Set(n => n.UpdatedAt, DateTime.UtcNow);

            var result = await Collection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Notification not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
            return Result.Failure<bool>($"Failed to mark notification as read: {ex.Message}");
        }
    }

    public async Task<Result<bool>> MarkAllAsReadAsync(Guid userId)
    {
        try
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.RecipientId, userId) &
                         Builders<Notification>.Filter.Eq(n => n.IsRead, false);

            var update = Builders<Notification>.Update
                .Set(n => n.IsRead, true)
                .Set(n => n.ReadAt, DateTime.UtcNow)
                .Set(n => n.UpdatedAt, DateTime.UtcNow);

            await Collection.UpdateManyAsync(filter, update);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read for user {UserId}", userId);
            return Result.Failure<bool>($"Failed to mark all notifications as read: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteNotificationAsync(string notificationId, Guid userId)
    {
        try
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId) &
                         Builders<Notification>.Filter.Eq(n => n.RecipientId, userId);

            var result = await Collection.DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
            {
                return Result.Failure<bool>("Notification not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting notification {NotificationId}", notificationId);
            return Result.Failure<bool>($"Failed to delete notification: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAllNotificationsAsync(Guid userId)
    {
        try
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.RecipientId, userId);
            await Collection.DeleteManyAsync(filter);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting all notifications for user {UserId}", userId);
            return Result.Failure<bool>($"Failed to delete all notifications: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateDeliveryStatusAsync(string notificationId, DeliveryStatus status)
    {
        try
        {
            var filter = Builders<Notification>.Filter.Eq(n => n.Id, notificationId);
            var update = Builders<Notification>.Update
                .Set(n => n.DeliveryStatus, status)
                .Set(n => n.UpdatedAt, DateTime.UtcNow);

            var result = await Collection.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
            {
                return Result.Failure<bool>("Notification not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating delivery status for notification {NotificationId}", notificationId);
            return Result.Failure<bool>($"Failed to update delivery status: {ex.Message}");
        }
    }

    // Notification Preferences
    public async Task<Result<NotificationPreferences>> GetUserPreferencesAsync(Guid userId)
    {
        try
        {
            var preferences = await _preferencesCollection
                .Find(p => p.UserId == userId)
                .FirstOrDefaultAsync();

            if (preferences == null)
            {
                // Create default preferences
                preferences = new NotificationPreferences
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await _preferencesCollection.InsertOneAsync(preferences);
            }

            return Result<NotificationPreferences>.Success(preferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting preferences for user {UserId}", userId);
            return Result.Failure<NotificationPreferences>($"Failed to get preferences: {ex.Message}");
        }
    }

    public async Task<Result<NotificationPreferences>> CreateOrUpdatePreferencesAsync(NotificationPreferences preferences)
    {
        try
        {
            preferences.UpdatedAt = DateTime.UtcNow;

            var filter = Builders<NotificationPreferences>.Filter.Eq(p => p.UserId, preferences.UserId);
            var options = new ReplaceOptions { IsUpsert = true };

            await _preferencesCollection.ReplaceOneAsync(filter, preferences, options);

            _logger.LogInformation("Updated notification preferences for user {UserId}", preferences.UserId);
            return Result<NotificationPreferences>.Success(preferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating preferences for user {UserId}", preferences.UserId);
            return Result.Failure<NotificationPreferences>($"Failed to update preferences: {ex.Message}");
        }
    }

    public async Task<Result<bool>> IsNotificationEnabledAsync(Guid userId, NotificationType type, DeliveryChannel channel)
    {
        try
        {
            var preferencesResult = await GetUserPreferencesAsync(userId);
            if (!preferencesResult.IsSuccess)
            {
                return Result<bool>.Success(true); // Default to enabled if preferences not found
            }

            var preferences = preferencesResult.Value;

            // Check if notifications are muted
            if (preferences.MuteUntil.HasValue && preferences.MuteUntil.Value > DateTime.UtcNow)
            {
                return Result<bool>.Success(false);
            }

            // Check quiet hours
            if (preferences.QuietHoursStart.HasValue && preferences.QuietHoursEnd.HasValue)
            {
                var now = DateTime.UtcNow.TimeOfDay;
                var start = preferences.QuietHoursStart.Value;
                var end = preferences.QuietHoursEnd.Value;

                bool inQuietHours;
                if (start < end)
                {
                    // Normal case: 22:00 to 08:00
                    inQuietHours = now >= start && now < end;
                }
                else
                {
                    // Crosses midnight: 22:00 to 02:00
                    inQuietHours = now >= start || now < end;
                }

                if (inQuietHours && channel != DeliveryChannel.InApp)
                {
                    return Result<bool>.Success(false);
                }
            }

            // Check global channel settings
            var globalEnabled = channel switch
            {
                DeliveryChannel.InApp => preferences.EnableInApp,
                DeliveryChannel.Push => preferences.EnablePush,
                DeliveryChannel.Email => preferences.EnableEmail,
                DeliveryChannel.SMS => preferences.EnableSMS,
                _ => true
            };

            if (!globalEnabled)
            {
                return Result<bool>.Success(false);
            }

            // Check type-specific settings
            if (preferences.NotificationTypes.TryGetValue(type, out var typeSettings))
            {
                var typeEnabled = channel switch
                {
                    DeliveryChannel.InApp => typeSettings.InApp,
                    DeliveryChannel.Push => typeSettings.Push,
                    DeliveryChannel.Email => typeSettings.Email,
                    DeliveryChannel.SMS => typeSettings.SMS,
                    _ => true
                };

                return Result<bool>.Success(typeEnabled);
            }

            return Result<bool>.Success(true); // Default to enabled if no specific settings
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking notification enabled for user {UserId}", userId);
            return Result.Failure<bool>($"Failed to check notification enabled: {ex.Message}");
        }
    }

    // Device Token Management
    public async Task<Result<DeviceToken>> RegisterDeviceTokenAsync(DeviceToken deviceToken)
    {
        try
        {
            deviceToken.CreatedAt = DateTime.UtcNow;
            deviceToken.UpdatedAt = DateTime.UtcNow;
            deviceToken.LastUsedAt = DateTime.UtcNow;
            deviceToken.IsActive = true;

            // Check if token already exists
            var existingToken = await _deviceTokensCollection
                .Find(d => d.UserId == deviceToken.UserId && d.Token == deviceToken.Token)
                .FirstOrDefaultAsync();

            if (existingToken != null)
            {
                // Update existing token
                var filter = Builders<DeviceToken>.Filter.Eq(d => d.Id, existingToken.Id);
                var update = Builders<DeviceToken>.Update
                    .Set(d => d.IsActive, true)
                    .Set(d => d.LastUsedAt, DateTime.UtcNow)
                    .Set(d => d.UpdatedAt, DateTime.UtcNow)
                    .Set(d => d.DeviceName, deviceToken.DeviceName)
                    .Set(d => d.AppVersion, deviceToken.AppVersion);

                await _deviceTokensCollection.UpdateOneAsync(filter, update);
                return Result<DeviceToken>.Success(existingToken);
            }

            await _deviceTokensCollection.InsertOneAsync(deviceToken);
            _logger.LogInformation("Registered device token for user {UserId}", deviceToken.UserId);
            return Result<DeviceToken>.Success(deviceToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering device token for user {UserId}", deviceToken.UserId);
            return Result.Failure<DeviceToken>($"Failed to register device token: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UnregisterDeviceTokenAsync(string tokenId, Guid userId)
    {
        try
        {
            var filter = Builders<DeviceToken>.Filter.Eq(d => d.Id, tokenId) &
                         Builders<DeviceToken>.Filter.Eq(d => d.UserId, userId);

            var result = await _deviceTokensCollection.DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
            {
                return Result.Failure<bool>("Device token not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering device token {TokenId}", tokenId);
            return Result.Failure<bool>($"Failed to unregister device token: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UnregisterDeviceByTokenAsync(string token, Guid userId)
    {
        try
        {
            var filter = Builders<DeviceToken>.Filter.Eq(d => d.Token, token) &
                         Builders<DeviceToken>.Filter.Eq(d => d.UserId, userId);

            var result = await _deviceTokensCollection.DeleteOneAsync(filter);

            if (result.DeletedCount == 0)
            {
                return Result.Failure<bool>("Device token not found");
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unregistering device token by value for user {UserId}", userId);
            return Result.Failure<bool>($"Failed to unregister device token: {ex.Message}");
        }
    }

    public async Task<Result<List<DeviceToken>>> GetUserDeviceTokensAsync(Guid userId, bool activeOnly = true)
    {
        try
        {
            var filterBuilder = Builders<DeviceToken>.Filter;
            var filter = filterBuilder.Eq(d => d.UserId, userId);

            if (activeOnly)
            {
                filter &= filterBuilder.Eq(d => d.IsActive, true);
            }

            var tokens = await _deviceTokensCollection
                .Find(filter)
                .SortByDescending(d => d.LastUsedAt)
                .ToListAsync();

            return Result<List<DeviceToken>>.Success(tokens);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device tokens for user {UserId}", userId);
            return Result.Failure<List<DeviceToken>>($"Failed to get device tokens: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateDeviceTokenLastUsedAsync(string tokenId)
    {
        try
        {
            var filter = Builders<DeviceToken>.Filter.Eq(d => d.Id, tokenId);
            var update = Builders<DeviceToken>.Update
                .Set(d => d.LastUsedAt, DateTime.UtcNow)
                .Set(d => d.UpdatedAt, DateTime.UtcNow);

            await _deviceTokensCollection.UpdateOneAsync(filter, update);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating device token last used {TokenId}", tokenId);
            return Result.Failure<bool>($"Failed to update device token: {ex.Message}");
        }
    }

    // Bulk Operations
    public async Task<Result<List<Notification>>> CreateBulkNotificationsAsync(List<Notification> notifications)
    {
        try
        {
            var now = DateTime.UtcNow;
            foreach (var notification in notifications)
            {
                notification.CreatedAt = now;
                notification.UpdatedAt = now;
            }

            await Collection.InsertManyAsync(notifications);
            _logger.LogInformation("Created {Count} notifications in bulk", notifications.Count);
            return Result<List<Notification>>.Success(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating bulk notifications");
            return Result.Failure<List<Notification>>($"Failed to create bulk notifications: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteExpiredNotificationsAsync()
    {
        try
        {
            var filter = Builders<Notification>.Filter.And(
                Builders<Notification>.Filter.Ne(n => n.ExpiresAt, null),
                Builders<Notification>.Filter.Lt(n => n.ExpiresAt, DateTime.UtcNow)
            );

            var result = await Collection.DeleteManyAsync(filter);
            _logger.LogInformation("Deleted {Count} expired notifications", result.DeletedCount);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting expired notifications");
            return Result.Failure<bool>($"Failed to delete expired notifications: {ex.Message}");
        }
    }
}
