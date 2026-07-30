using System.Text.RegularExpressions;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderCredentialing.ValueObjects;

public sealed record LicenseNumber
{
    private static readonly Regex LicenseNumberRegex = new(@"^[A-Za-z0-9\-]+$", RegexOptions.Compiled);

    public string Value { get; }

    private LicenseNumber(string value)
    {
        Value = value;
    }

    public static Result<LicenseNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<LicenseNumber>.Failure("License number cannot be null or whitespace");

        var trimmed = value.Trim();

        if (trimmed.Length < 1 || trimmed.Length > 50)
            return Result<LicenseNumber>.Failure("License number must be between 1 and 50 characters");

        if (!LicenseNumberRegex.IsMatch(trimmed))
            return Result<LicenseNumber>.Failure("License number must contain only alphanumeric characters and hyphens");

        return Result<LicenseNumber>.Success(new LicenseNumber(trimmed));
    }

    public static implicit operator string(LicenseNumber licenseNumber) => licenseNumber.Value;

    public override string ToString() => Value;
}
