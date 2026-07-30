using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.PlacementMatching;

public sealed record PositionId
{
    public uint Value { get; }

    private PositionId(uint value)
    {
        Value = value;
    }

    public static Result<PositionId> Create(uint value)
    {
        if (value == 0)
            return Result<PositionId>.Failure("PositionId cannot be zero");

        return Result<PositionId>.Success(new PositionId(value));
    }

    
    public static implicit operator uint(PositionId id) => id.Value;

    public override string ToString() => Value.ToString();
}
