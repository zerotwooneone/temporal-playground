namespace TemporalDDD.Domain.SharedKernel;

public sealed record AssignmentId
{
    public Guid Value { get; }

    private AssignmentId(Guid value)
    {
        Value = value;
    }

    public static AssignmentId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AssignmentId cannot be empty", nameof(value));

        return new AssignmentId(value);
    }

    public static AssignmentId New() => new(Guid.NewGuid());

    public static implicit operator Guid(AssignmentId id) => id.Value;

    public override string ToString() => Value.ToString();
}
