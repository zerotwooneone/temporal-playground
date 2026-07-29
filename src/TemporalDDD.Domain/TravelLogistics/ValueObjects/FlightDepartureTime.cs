namespace TemporalDDD.Domain.TravelLogistics.ValueObjects;

public sealed record FlightDepartureTime
{
    public DateTime Value { get; }

    private FlightDepartureTime(DateTime value)
    {
        Value = value;
    }

    public static FlightDepartureTime Create(DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Flight departure time must be in UTC", nameof(value));

        return new FlightDepartureTime(value);
    }

    public static FlightDepartureTime CreateUtcNow() => new(DateTime.UtcNow);

    public bool IsPast() => Value < DateTime.UtcNow;

    public static implicit operator DateTime(FlightDepartureTime departureTime) => departureTime.Value;

    public override string ToString() => Value.ToString("yyyy-MM-dd HH:mm:ss UTC");
}
