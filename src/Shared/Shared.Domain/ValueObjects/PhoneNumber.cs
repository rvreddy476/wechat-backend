using System.Text.RegularExpressions;
namespace Shared.Domain.ValueObjects;

public sealed record PhoneNumber
{
    private static readonly Regex PhoneRegex = new(@"^\+[1-9]\d{1,14}$", RegexOptions.Compiled);
    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    public static PhoneNumber? Create(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return null;
        var normalized = phoneNumber.Trim();
        return !PhoneRegex.IsMatch(normalized) ? null : new PhoneNumber(normalized);
    }

    public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
    public override string ToString() => Value;
}
