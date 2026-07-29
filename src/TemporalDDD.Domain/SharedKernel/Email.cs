using System.Text.RegularExpressions;

namespace TemporalDDD.Domain.SharedKernel;

public sealed record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be null or whitespace", nameof(value));

        var trimmed = value.Trim();
        var normalized = trimmed.ToLowerInvariant();

        if (normalized.Length > 255)
            throw new ArgumentException("Email cannot exceed 255 characters", nameof(value));

        if (!EmailRegex.IsMatch(normalized))
            throw new ArgumentException("Email format is invalid", nameof(value));

        return new Email(normalized);
    }

    public static implicit operator string(Email email) => email.Value;

    public override string ToString() => Value;
}
