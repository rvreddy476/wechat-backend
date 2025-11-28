using System.Text.RegularExpressions;
using Shared.Domain.Constants;

namespace Shared.Domain.ValueObjects;

/// <summary>
/// Username value object with validation
/// Alphanumeric with underscores, 3-50 characters
/// </summary>
public sealed record Username
{
    private static readonly Regex UsernameRegex = new(
        @"^[a-zA-Z0-9_]+$",
        RegexOptions.Compiled);

    public string Value { get; }

    private Username(string value)
    {
        Value = value;
    }

    public static Username? Create(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        var trimmed = username.Trim();

        if (trimmed.Length < SharedConstants.UserProfile.MinUsernameLength ||
            trimmed.Length > SharedConstants.UserProfile.MaxUsernameLength)
            return null;

        if (!UsernameRegex.IsMatch(trimmed))
            return null;

        return new Username(trimmed);
    }

    public static implicit operator string(Username username) => username.Value;
    public override string ToString() => Value;
}
