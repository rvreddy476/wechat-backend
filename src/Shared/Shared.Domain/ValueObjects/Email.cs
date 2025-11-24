using System.Text.RegularExpressions;

namespace Shared.Domain.ValueObjects;

/// <summary>
/// Email value object with validation
/// </summary>
public sealed record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email? Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(normalizedEmail))
            return null;

        return new Email(normalizedEmail);
    }

    public static implicit operator string(Email email) => email.Value;
    public override string ToString() => Value;
}
