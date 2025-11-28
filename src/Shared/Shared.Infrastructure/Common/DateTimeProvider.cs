namespace Shared.Infrastructure.Common;

/// <summary>
/// Interface for DateTime provider (useful for testing)
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
    DateTime Now { get; }
}

/// <summary>
/// System DateTime provider implementation
/// </summary>
public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTime Now => DateTime.Now;
}
