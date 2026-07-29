namespace TemporalDDD.Domain.SharedKernel;

public sealed record FlightBookingId
{
    public uint Value { get; }

    private FlightBookingId(uint value) => Value = value;

    public static FlightBookingId Create(uint value)
    {
        if (value == 0)
            throw new ArgumentException("FlightBookingId cannot be zero", nameof(value));
        return new FlightBookingId(value);
    }

    public static FlightBookingId FromDatabase(uint value) => new(value);
    public static implicit operator uint(FlightBookingId id) => id.Value;
    public override string ToString() => Value.ToString();
}
