using System.Text.RegularExpressions;
using TemporalDDD.Domain.SharedKernel;

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

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Email>.Failure("Email cannot be null or whitespace");

        var trimmed = value.Trim();
        var normalized = trimmed.ToLowerInvariant();

        if (normalized.Length > 255)
            return Result<Email>.Failure("Email cannot exceed 255 characters");

        if (!EmailRegex.IsMatch(normalized))
            return Result<Email>.Failure("Email format is invalid");

        return Result<Email>.Success(new Email(normalized));
    }

    public static implicit operator string(Email email) => email.Value;

    public override string ToString() => Value;
}
