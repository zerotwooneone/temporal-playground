using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.TimesheetProcessing.ValueObjects;

public sealed record Hours
{
    public decimal Value { get; }

    private Hours(decimal value)
    {
        Value = value;
    }

    public static Result<Hours> Create(decimal value)
    {
        if (value < 0.0m || value > 160.0m)
            return Result<Hours>.Failure("Hours must be between 0.0 and 160.0");

        return Result<Hours>.Success(new Hours(value));
    }

    public static Hours Zero() => new(0.0m);
    public static Hours FullTime() => new(160.0m);

    public bool IsFullTime() => Value >= 160.0m;
    public bool IsPartTime() => Value > 0.0m && Value < 160.0m;

    public static implicit operator decimal(Hours hours) => hours.Value;

    public override string ToString() => $"{Value:F2}h";
}
