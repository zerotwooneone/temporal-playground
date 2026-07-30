using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.TravelLogistics.ValueObjects;

public sealed record FlightDepartureTime
{
    public DateTimeOffset Value { get; }

    private FlightDepartureTime(DateTimeOffset value)
    {
        Value = value;
    }

    public static Result<FlightDepartureTime> Create(DateTimeOffset value)
    {
        if (value.Offset != TimeSpan.Zero)
            return Result<FlightDepartureTime>.Failure("Flight departure time must be in UTC");

        return Result<FlightDepartureTime>.Success(new FlightDepartureTime(value));
    }

    public static FlightDepartureTime CreateUtcNow() => new(DateTimeOffset.UtcNow);

    public bool IsPast() => Value < DateTimeOffset.UtcNow;

    public static implicit operator DateTimeOffset(FlightDepartureTime departureTime) => departureTime.Value;

    public override string ToString() => Value.ToString("yyyy-MM-dd HH:mm:ss UTC");
}
