namespace Realtime.Api.Services;

public interface IPresenceService
{
    Task SetUserOnlineAsync(Guid userId, string connectionId);
    Task<bool> RemoveConnectionAsync(Guid userId, string connectionId);
    Task<bool> IsUserOnlineAsync(Guid userId);
    Task UpdateUserStatusAsync(Guid userId, string status);
    Task<string?> GetUserStatusAsync(Guid userId);
    Task<DateTime?> GetLastSeenAsync(Guid userId);
    Task UpdateHeartbeatAsync(Guid userId, string connectionId);
    Task<List<Guid>> GetOnlineUsersAsync();
    Task<int> GetOnlineCountAsync();
    Task CleanupStaleConnectionsAsync();
}
