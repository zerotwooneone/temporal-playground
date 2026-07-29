using System.Text.RegularExpressions;

namespace TemporalDDD.Domain.ProviderCredentialing.ValueObjects;

public sealed record LicenseNumber
{
    private static readonly Regex LicenseNumberRegex = new(@"^[A-Za-z0-9\-]+$", RegexOptions.Compiled);

    public string Value { get; }

    private LicenseNumber(string value)
    {
        Value = value;
    }

    public static LicenseNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("License number cannot be null or whitespace", nameof(value));

        var trimmed = value.Trim();

        if (trimmed.Length < 1 || trimmed.Length > 50)
            throw new ArgumentException("License number must be between 1 and 50 characters", nameof(value));

        if (!LicenseNumberRegex.IsMatch(trimmed))
            throw new ArgumentException("License number must contain only alphanumeric characters and hyphens", nameof(value));

        return new LicenseNumber(trimmed);
    }

    public static implicit operator string(LicenseNumber licenseNumber) => licenseNumber.Value;

    public override string ToString() => Value;
}
