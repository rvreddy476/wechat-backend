using StackExchange.Redis;
using System.Text.Json;

namespace Shared.Infrastructure.Redis;

public interface IRedisService
{
    Task<T?> GetAsync<T>(string key);
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<bool> DeleteAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task<long> IncrementAsync(string key, long value = 1);
    Task<long> DecrementAsync(string key, long value = 1);
    Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null);
    Task<string?> GetStringAsync(string key);
}

public class RedisService : IRedisService
{
    private readonly IDatabase _database;

    public RedisService(IConnectionMultiplexer connectionMultiplexer, int databaseNumber = 0)
    {
        _database = connectionMultiplexer.GetDatabase(databaseNumber);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);

        if (!value.HasValue)
            return default;

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        var serializedValue = JsonSerializer.Serialize(value);
        return await _database.StringSetAsync(key, serializedValue, expiry);
    }

    public async Task<bool> DeleteAsync(string key)
    {
        return await _database.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }

    public async Task<long> IncrementAsync(string key, long value = 1)
    {
        return await _database.StringIncrementAsync(key, value);
    }

    public async Task<long> DecrementAsync(string key, long value = 1)
    {
        return await _database.StringDecrementAsync(key, value);
    }

    public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null)
    {
        return await _database.StringSetAsync(key, value, expiry);
    }

    public async Task<string?> GetStringAsync(string key)
    {
        var value = await _database.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }
}
