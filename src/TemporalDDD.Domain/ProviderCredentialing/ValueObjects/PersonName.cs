using System.Text.RegularExpressions;

namespace TemporalDDD.Domain.ProviderCredentialing.ValueObjects;

public sealed record PersonName
{
    private static readonly Regex NameRegex = new(@"^[a-zA-Z\s\-'\.]+$", RegexOptions.Compiled);

    public string Value { get; }

    private PersonName(string value)
    {
        Value = value;
    }

    public static PersonName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Person name cannot be null or whitespace", nameof(value));

        var trimmed = value.Trim();

        if (trimmed.Length < 1 || trimmed.Length > 255)
            throw new ArgumentException("Person name must be between 1 and 255 characters", nameof(value));

        if (!NameRegex.IsMatch(trimmed))
            throw new ArgumentException("Person name must contain only letters, spaces, hyphens, apostrophes, and periods", nameof(value));

        return new PersonName(trimmed);
    }

    public static implicit operator string(PersonName personName) => personName.Value;

    public override string ToString() => Value;
}
