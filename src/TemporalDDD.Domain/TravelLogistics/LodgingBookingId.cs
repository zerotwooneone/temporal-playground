namespace TemporalDDD.Domain.TravelLogistics;

public sealed record LodgingBookingId
{
    public uint Value { get; }

    private LodgingBookingId(uint value) => Value = value;

    public static LodgingBookingId Create(uint value)
    {
        if (value == 0)
            throw new ArgumentException("LodgingBookingId cannot be zero", nameof(value));
        return new LodgingBookingId(value);
    }

    public static LodgingBookingId FromDatabase(uint value) => new(value);
    public static implicit operator uint(LodgingBookingId id) => id.Value;
    public override string ToString() => Value.ToString();
}
