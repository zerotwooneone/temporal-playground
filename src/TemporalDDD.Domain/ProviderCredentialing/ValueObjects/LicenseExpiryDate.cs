using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderCredentialing.ValueObjects;

public sealed record LicenseExpiryDate
{
    public DateOnly Value { get; }

    internal LicenseExpiryDate(DateOnly value)
    {
        Value = value;
    }
    
    public static Result<LicenseExpiryDate> Create(DateOnly value)
    {
        if(value == default) return Result<LicenseExpiryDate>.Failure("Expiry date cannot be default");
        return Result<LicenseExpiryDate>.Success(new LicenseExpiryDate(value));
    }

    public bool IsExpired(DateOnly today) => Value <= today;

    public int DaysUntilExpiry(DateOnly today)
    {
        var days = Value.DayNumber - today.DayNumber;
        return Math.Max(0, days);
    }

    public int DaysSinceExpiry(DateOnly today)
    {
        var days = today.DayNumber - Value.DayNumber;
        return Math.Max(0, days);
    }

    public override string ToString() => Value.ToString("yyyy-MM-dd");
}