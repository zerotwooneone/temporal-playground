namespace TemporalDDD.Domain.SharedKernel;

public sealed record PositionId
{
    public Guid Value { get; }

    private PositionId(Guid value)
    {
        Value = value;
    }

    public static PositionId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("PositionId cannot be empty", nameof(value));

        return new PositionId(value);
    }

    public static PositionId New() => new(Guid.NewGuid());

    public static implicit operator Guid(PositionId id) => id.Value;

    public override string ToString() => Value.ToString();
}
