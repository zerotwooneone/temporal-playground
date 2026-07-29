namespace TemporalDDD.Domain.SharedKernel;

public sealed record FlightBookingPublicId
{
    private const string Prefix = "FLT";
    public Guid Value { get; }

    private FlightBookingPublicId(Guid value)
    {
        Value = value;
    }

    public static FlightBookingPublicId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("FlightBookingPublicId cannot be empty", nameof(value));

        return new FlightBookingPublicId(value);
    }

    public static FlightBookingPublicId New() => new(Guid.NewGuid());

    public static implicit operator Guid(FlightBookingPublicId id) => id.Value;

    public override string ToString() => $"{Prefix}_{Value:N}";
}
