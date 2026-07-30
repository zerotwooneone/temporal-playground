using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderCredentialing.ValueObjects;

public sealed record LicenseExpiryDate
{
    public DateTimeOffset Value { get; }

    private LicenseExpiryDate(DateTimeOffset value)
    {
        Value = value;
    }

    public static Result<LicenseExpiryDate> Create(DateTimeOffset value)
    {
        if (value <= DateTimeOffset.UtcNow)
            return Result<LicenseExpiryDate>.Failure("License expiry date must be in the future");

        return Result<LicenseExpiryDate>.Success(new LicenseExpiryDate(value));
    }

    public bool IsExpired() => Value <= DateTimeOffset.UtcNow;

    public int DaysUntilExpiry() => Math.Max(0, (Value - DateTimeOffset.UtcNow).Days);

    public static implicit operator DateTimeOffset(LicenseExpiryDate expiryDate) => expiryDate.Value;

    public override string ToString() => Value.ToString("yyyy-MM-dd");
}
