namespace TemporalDDD.Domain.SharedKernel;

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

    // Factory method for rehydration from database
    public static AssignmentId FromDatabase(uint value) => new(value);

    public static implicit operator uint(AssignmentId id) => id.Value;

    public override string ToString() => Value.ToString();
}
