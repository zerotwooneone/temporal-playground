using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.TimesheetProcessing.ValueObjects;

public sealed record HourlyRate
{
    public decimal Value { get; }

    private HourlyRate(decimal value)
    {
        Value = value;
    }

    public static HourlyRate Create(decimal value)
    {
        if (value <= 0.0m)
            throw new ArgumentException("Hourly rate must be greater than zero", nameof(value));

        return new HourlyRate(value);
    }

    public Money CalculatePay(Hours hours) => Money.Create(hours.Value * Value);

    public static implicit operator decimal(HourlyRate hourlyRate) => hourlyRate.Value;

    public override string ToString() => $"${Value:F2}/hr";
}
