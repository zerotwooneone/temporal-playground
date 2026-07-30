namespace TemporalDDD.Domain.TravelLogistics;

public sealed record LodgingBookingPublicId
{
    private const string Prefix = "LOD";
    public Guid Value { get; }

    private LodgingBookingPublicId(Guid value)
    {
        Value = value;
    }

    public static LodgingBookingPublicId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("LodgingBookingPublicId cannot be empty", nameof(value));

        return new LodgingBookingPublicId(value);
    }

    public static LodgingBookingPublicId New() => new(Guid.NewGuid());

    public static implicit operator Guid(LodgingBookingPublicId id) => id.Value;

    public override string ToString() => $"{Prefix}_{Value:N}";
}
