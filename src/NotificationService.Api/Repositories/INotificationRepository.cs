using NotificationService.Api.Models;
using Shared.Domain.Common;

namespace NotificationService.Api.Repositories;

public interface INotificationRepository
{
    // Notification Management
    Task<Result<Notification>> CreateNotificationAsync(Notification notification);
    Task<Result<Notification>> GetNotificationByIdAsync(string notificationId);
    Task<Result<List<Notification>>> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 20, bool unreadOnly = false, NotificationType? type = null);
    Task<Result<int>> GetUnreadCountAsync(Guid userId);
    Task<Result<bool>> MarkAsReadAsync(string notificationId, Guid userId);
    Task<Result<bool>> MarkAllAsReadAsync(Guid userId);
    Task<Result<bool>> DeleteNotificationAsync(string notificationId, Guid userId);
    Task<Result<bool>> DeleteAllNotificationsAsync(Guid userId);
    Task<Result<bool>> UpdateDeliveryStatusAsync(string notificationId, DeliveryStatus status);

    // Notification Preferences
    Task<Result<NotificationPreferences>> GetUserPreferencesAsync(Guid userId);
    Task<Result<NotificationPreferences>> CreateOrUpdatePreferencesAsync(NotificationPreferences preferences);
    Task<Result<bool>> IsNotificationEnabledAsync(Guid userId, NotificationType type, DeliveryChannel channel);

    // Device Token Management
    Task<Result<DeviceToken>> RegisterDeviceTokenAsync(DeviceToken deviceToken);
    Task<Result<bool>> UnregisterDeviceTokenAsync(string tokenId, Guid userId);
    Task<Result<bool>> UnregisterDeviceByTokenAsync(string token, Guid userId);
    Task<Result<List<DeviceToken>>> GetUserDeviceTokensAsync(Guid userId, bool activeOnly = true);
    Task<Result<bool>> UpdateDeviceTokenLastUsedAsync(string tokenId);

    // Bulk Operations
    Task<Result<List<Notification>>> CreateBulkNotificationsAsync(List<Notification> notifications);
    Task<Result<bool>> DeleteExpiredNotificationsAsync();
}
