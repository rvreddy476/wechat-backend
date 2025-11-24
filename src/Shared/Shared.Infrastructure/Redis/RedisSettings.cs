namespace Shared.Infrastructure.Redis;

public class RedisSettings
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public int DatabaseNumber { get; set; } = 0;
}
