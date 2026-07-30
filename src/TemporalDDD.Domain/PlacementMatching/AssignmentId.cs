using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.PlacementMatching;

public sealed record AssignmentId
{
    public uint Value { get; }

    private AssignmentId(uint value)
    {
        Value = value;
    }

    public static Result<AssignmentId> Create(uint value)
    {
        if (value == 0)
            return Result<AssignmentId>.Failure("AssignmentId cannot be zero");

        return Result<AssignmentId>.Success(new AssignmentId(value));
    }

    public static implicit operator uint(AssignmentId id) => id.Value;

    public override string ToString() => Value.ToString();
}
