using NotificationService.Api.Models;
using Shared.Domain.Common;

namespace NotificationService.Api.Services;

public interface INotificationService
{
    Task<Result<Notification>> SendNotificationAsync(Notification notification);
    Task<Result<List<Notification>>> SendBulkNotificationsAsync(List<Notification> notifications);
    Task<Result<bool>> SendToChannelAsync(Notification notification, DeliveryChannel channel);
}
