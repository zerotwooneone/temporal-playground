using System.Text.RegularExpressions;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderCredentialing.ValueObjects;

public sealed record PersonName
{
    private static readonly Regex NameRegex = new(@"^[a-zA-Z\s\-'\.]+$", RegexOptions.Compiled);

    public string Value { get; }

    private PersonName(string value)
    {
        Value = value;
    }

    public static Result<PersonName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<PersonName>.Failure("Person name cannot be null or whitespace");

        var trimmed = value.Trim();

        if (trimmed.Length < 1 || trimmed.Length > 255)
            return Result<PersonName>.Failure("Person name must be between 1 and 255 characters");

        if (!NameRegex.IsMatch(trimmed))
            return Result<PersonName>.Failure("Person name must contain only letters, spaces, hyphens, apostrophes, and periods");

        return Result<PersonName>.Success(new PersonName(trimmed));
    }

    public static implicit operator string(PersonName personName) => personName.Value;

    public override string ToString() => Value;
}
