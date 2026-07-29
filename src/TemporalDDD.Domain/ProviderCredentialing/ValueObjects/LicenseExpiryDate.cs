namespace TemporalDDD.Domain.ProviderCredentialing.ValueObjects;

public sealed record LicenseExpiryDate
{
    public DateTime Value { get; }

    private LicenseExpiryDate(DateTime value)
    {
        Value = value;
    }

    public static LicenseExpiryDate Create(DateTime value)
    {
        if (value <= DateTime.UtcNow)
            throw new ArgumentException("License expiry date must be in the future", nameof(value));

        return new LicenseExpiryDate(value);
    }

    public bool IsExpired() => Value <= DateTime.UtcNow;

    public int DaysUntilExpiry() => Math.Max(0, (Value - DateTime.UtcNow).Days);

    public static implicit operator DateTime(LicenseExpiryDate expiryDate) => expiryDate.Value;

    public override string ToString() => Value.ToString("yyyy-MM-dd");
}
