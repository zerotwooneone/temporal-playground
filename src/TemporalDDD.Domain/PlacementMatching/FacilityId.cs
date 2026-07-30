using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.PlacementMatching;

public sealed record FacilityId
{
    public uint Value { get; }

    private FacilityId(uint value)
    {
        Value = value;
    }

    public static Result<FacilityId> Create(uint value)
    {
        if (value == 0)
            return Result<FacilityId>.Failure("FacilityId cannot be zero");

        return Result<FacilityId>.Success(new FacilityId(value));
    }

    public static implicit operator uint(FacilityId id) => id.Value;

    public override string ToString() => Value.ToString();
}
