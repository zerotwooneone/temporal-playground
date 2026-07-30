namespace TemporalDDD.Domain.TravelLogistics;

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

    public static FlightBookingPublicId FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("FlightBookingPublicId cannot be null or whitespace", nameof(value));

        var parts = value.Split('_');
        if (parts.Length != 2)
            throw new ArgumentException("FlightBookingPublicId must be in format 'PREFIX_Guid'", nameof(value));

        if (parts[0] != Prefix)
            throw new ArgumentException($"FlightBookingPublicId must have prefix '{Prefix}'", nameof(value));

        var guidValue = Guid.Parse(parts[1]);
        if (guidValue == Guid.Empty)
            throw new ArgumentException("FlightBookingPublicId cannot be empty", nameof(value));

        return new FlightBookingPublicId(guidValue);
    }

    public static FlightBookingPublicId New() => new(Guid.NewGuid());

    public static implicit operator Guid(FlightBookingPublicId id) => id.Value;

    public override string ToString() => $"{Prefix}_{Value:N}";
}
