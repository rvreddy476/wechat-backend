using System.Text.RegularExpressions;

namespace Shared.Domain.ValueObjects;

/// <summary>
/// PhoneNumber value object with validation
/// Supports international phone numbers in E.164 format
/// </summary>
public sealed record PhoneNumber
{
    // E.164 format: +[country code][subscriber number]
    // Example: +14155552671, +919876543210
    private static readonly Regex PhoneRegex = new(
        @"^\+[1-9]\d{1,14}$",
        RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber? Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return null;

        var normalized = phoneNumber.Trim();

        if (!PhoneRegex.IsMatch(normalized))
            return null;

        return new PhoneNumber(normalized);
    }

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
    public override string ToString() => Value;
}
