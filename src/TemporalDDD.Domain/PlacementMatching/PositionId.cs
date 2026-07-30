namespace TemporalDDD.Domain.PlacementMatching;

public sealed record PositionId
{
    public uint Value { get; }

    private PositionId(uint value)
    {
        Value = value;
    }

    public static PositionId Create(uint value)
    {
        if (value == 0)
            throw new ArgumentException("PositionId cannot be zero", nameof(value));

        return new PositionId(value);
    }

    // Factory method for rehydration from database
    public static PositionId FromDatabase(uint value) => new(value);

    public static implicit operator uint(PositionId id) => id.Value;

    public override string ToString() => Value.ToString();
}
