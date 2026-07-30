namespace TemporalDDD.Domain.PlacementMatching;

public sealed record FacilityId
{
    public uint Value { get; }

    private FacilityId(uint value)
    {
        Value = value;
    }

    public static FacilityId Create(uint value)
    {
        if (value == 0)
            throw new ArgumentException("FacilityId cannot be zero", nameof(value));

        return new FacilityId(value);
    }

    // Factory method for rehydration from database
    public static FacilityId FromDatabase(uint value) => new(value);

    public static implicit operator uint(FacilityId id) => id.Value;

    public override string ToString() => Value.ToString();
}
