using StackExchange.Redis;

namespace Realtime.Api.Services;

public class PresenceService : IPresenceService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<PresenceService> _logger;
    private readonly int _onlineThresholdSeconds;

    public PresenceService(
        IConnectionMultiplexer redis,
        IConfiguration configuration,
        ILogger<PresenceService> logger)
    {
        _redis = redis;
        _logger = logger;
        _onlineThresholdSeconds = int.Parse(configuration["PresenceSettings:OnlineThresholdSeconds"] ?? "30");
    }

    private IDatabase GetDatabase() => _redis.GetDatabase();

    // Redis Keys
    private string UserConnectionsKey(Guid userId) => $"presence:connections:{userId}";
    private string UserStatusKey(Guid userId) => $"presence:status:{userId}";
    private string UserLastSeenKey(Guid userId) => $"presence:lastseen:{userId}";
    private string OnlineUsersKey() => "presence:online";
    private string ConnectionHeartbeatKey(string connectionId) => $"presence:heartbeat:{connectionId}";

    public async Task SetUserOnlineAsync(Guid userId, string connectionId)
    {
        try
        {
            var db = GetDatabase();
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Add connection to user's connections set
            await db.SetAddAsync(UserConnectionsKey(userId), connectionId);

            // Set connection heartbeat with TTL
            await db.StringSetAsync(
                ConnectionHeartbeatKey(connectionId),
                now.ToString(),
                TimeSpan.FromMinutes(10));

            // Add user to online users set
            await db.SortedSetAddAsync(OnlineUsersKey(), userId.ToString(), now);

            // Update last seen
            await db.StringSetAsync(UserLastSeenKey(userId), now.ToString());

            // Set default status if not exists
            var status = await db.StringGetAsync(UserStatusKey(userId));
            if (status.IsNullOrEmpty)
            {
                await db.StringSetAsync(UserStatusKey(userId), "online", TimeSpan.FromHours(24));
            }

            _logger.LogInformation("User {UserId} set online with connection {ConnectionId}", userId, connectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting user {UserId} online", userId);
        }
    }

    public async Task<bool> RemoveConnectionAsync(Guid userId, string connectionId)
    {
        try
        {
            var db = GetDatabase();

            // Remove connection from user's connections set
            await db.SetRemoveAsync(UserConnectionsKey(userId), connectionId);

            // Remove connection heartbeat
            await db.KeyDeleteAsync(ConnectionHeartbeatKey(connectionId));

            // Check if user has any remaining connections
            var connectionCount = await db.SetLengthAsync(UserConnectionsKey(userId));

            if (connectionCount == 0)
            {
                // No more connections, mark user as offline
                await db.SortedSetRemoveAsync(OnlineUsersKey(), userId.ToString());

                // Update last seen
                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                await db.StringSetAsync(UserLastSeenKey(userId), now.ToString());

                // Clear status (or set to offline)
                await db.StringSetAsync(UserStatusKey(userId), "offline", TimeSpan.FromHours(24));

                _logger.LogInformation("User {UserId} is now offline (no remaining connections)", userId);
                return true; // User is offline
            }

            _logger.LogInformation("User {UserId} removed connection {ConnectionId} but has {Count} remaining connections",
                userId, connectionId, connectionCount);
            return false; // User still has connections
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing connection {ConnectionId} for user {UserId}", connectionId, userId);
            return false;
        }
    }

    public async Task<bool> IsUserOnlineAsync(Guid userId)
    {
        try
        {
            var db = GetDatabase();

            // Check if user is in online users set
            var score = await db.SortedSetScoreAsync(OnlineUsersKey(), userId.ToString());
            if (!score.HasValue)
            {
                return false;
            }

            // Check if the last activity is within the online threshold
            var lastActivityTime = score.Value;
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var timeSinceLastActivity = now - lastActivityTime;

            return timeSinceLastActivity <= _onlineThresholdSeconds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if user {UserId} is online", userId);
            return false;
        }
    }

    public async Task UpdateUserStatusAsync(Guid userId, string status)
    {
        try
        {
            var db = GetDatabase();
            await db.StringSetAsync(UserStatusKey(userId), status, TimeSpan.FromHours(24));

            _logger.LogInformation("Updated status for user {UserId} to {Status}", userId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for user {UserId}", userId);
        }
    }

    public async Task<string?> GetUserStatusAsync(Guid userId)
    {
        try
        {
            var db = GetDatabase();
            var status = await db.StringGetAsync(UserStatusKey(userId));

            return status.HasValue ? status.ToString() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting status for user {UserId}", userId);
            return null;
        }
    }

    public async Task<DateTime?> GetLastSeenAsync(Guid userId)
    {
        try
        {
            var db = GetDatabase();
            var lastSeenString = await db.StringGetAsync(UserLastSeenKey(userId));

            if (!lastSeenString.HasValue)
            {
                return null;
            }

            if (long.TryParse(lastSeenString, out var lastSeenUnix))
            {
                return DateTimeOffset.FromUnixTimeSeconds(lastSeenUnix).UtcDateTime;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting last seen for user {UserId}", userId);
            return null;
        }
    }

    public async Task UpdateHeartbeatAsync(Guid userId, string connectionId)
    {
        try
        {
            var db = GetDatabase();
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Update connection heartbeat
            await db.StringSetAsync(
                ConnectionHeartbeatKey(connectionId),
                now.ToString(),
                TimeSpan.FromMinutes(10));

            // Update user's last activity in online users set
            await db.SortedSetAddAsync(OnlineUsersKey(), userId.ToString(), now);

            // Update last seen
            await db.StringSetAsync(UserLastSeenKey(userId), now.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating heartbeat for user {UserId}", userId);
        }
    }

    public async Task<List<Guid>> GetOnlineUsersAsync()
    {
        try
        {
            var db = GetDatabase();
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var threshold = now - _onlineThresholdSeconds;

            // Get users who were active within the threshold
            var onlineUserIds = await db.SortedSetRangeByScoreAsync(
                OnlineUsersKey(),
                start: threshold,
                stop: double.PositiveInfinity);

            return onlineUserIds
                .Select(id => Guid.TryParse(id, out var guid) ? guid : (Guid?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting online users");
            return new List<Guid>();
        }
    }

    public async Task<int> GetOnlineCountAsync()
    {
        try
        {
            var db = GetDatabase();
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var threshold = now - _onlineThresholdSeconds;

            var count = await db.SortedSetLengthAsync(
                OnlineUsersKey(),
                min: threshold,
                max: double.PositiveInfinity);

            return (int)count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting online count");
            return 0;
        }
    }

    public async Task CleanupStaleConnectionsAsync()
    {
        try
        {
            var db = GetDatabase();
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var threshold = now - (_onlineThresholdSeconds * 2); // 2x the threshold

            // Remove stale users from online set
            var removed = await db.SortedSetRemoveRangeByScoreAsync(
                OnlineUsersKey(),
                start: double.NegativeInfinity,
                stop: threshold);

            if (removed > 0)
            {
                _logger.LogInformation("Cleaned up {Count} stale connections", removed);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up stale connections");
        }
    }
}
