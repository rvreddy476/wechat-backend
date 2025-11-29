using System.Text.RegularExpressions;
namespace Shared.Domain.ValueObjects;

public sealed record Email
{
    private static readonly Regex EmailRegex = new(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.Compiled);
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email? Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;
        var normalized = email.Trim().ToLowerInvariant();
        return !EmailRegex.IsMatch(normalized) ? null : new Email(normalized);
    }

    public static implicit operator string(Email email) => email.Value;
    public override string ToString() => Value;
}
