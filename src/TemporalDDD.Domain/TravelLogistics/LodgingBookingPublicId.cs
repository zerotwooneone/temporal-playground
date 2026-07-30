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

    public static LodgingBookingPublicId FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("LodgingBookingPublicId cannot be null or whitespace", nameof(value));

        var parts = value.Split('_');
        if (parts.Length != 2)
            throw new ArgumentException("LodgingBookingPublicId must be in format 'PREFIX_Guid'", nameof(value));

        if (parts[0] != Prefix)
            throw new ArgumentException($"LodgingBookingPublicId must have prefix '{Prefix}'", nameof(value));

        var guidValue = Guid.Parse(parts[1]);
        if (guidValue == Guid.Empty)
            throw new ArgumentException("LodgingBookingPublicId cannot be empty", nameof(value));

        return new LodgingBookingPublicId(guidValue);
    }

    public static LodgingBookingPublicId New() => new(Guid.NewGuid());

    public static implicit operator Guid(LodgingBookingPublicId id) => id.Value;

    public override string ToString() => $"{Prefix}_{Value:N}";
}
