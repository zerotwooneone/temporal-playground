namespace TemporalDDD.Domain.PlacementMatching.ValueObjects;

public sealed record AggregateVersion
{
    public int Value { get; }

    private AggregateVersion(int value)
    {
        Value = value;
    }

    public static AggregateVersion Create(int value)
    {
        if (value < 0)
            throw new ArgumentException("Version cannot be negative", nameof(value));

        return new AggregateVersion(value);
    }

    public static AggregateVersion Initial() => new(1);
    public static AggregateVersion Zero() => new(0);

    public AggregateVersion Increment() => new(Value + 1);

    public static implicit operator int(AggregateVersion version) => version.Value;

    public override string ToString() => Value.ToString();
}
