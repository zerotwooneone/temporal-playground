namespace TemporalDDD.Domain.PlacementMatching;

public sealed record AssignmentId
{
    public uint Value { get; }

    private AssignmentId(uint value)
    {
        Value = value;
    }

    public static AssignmentId Create(uint value)
    {
        if (value == 0)
            throw new ArgumentException("AssignmentId cannot be zero", nameof(value));

        return new AssignmentId(value);
    }

    

    public static implicit operator uint(AssignmentId id) => id.Value;

    public override string ToString() => Value.ToString();
}
