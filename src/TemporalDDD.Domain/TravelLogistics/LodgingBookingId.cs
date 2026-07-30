using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.TravelLogistics;

public sealed record LodgingBookingId
{
    public uint Value { get; }

    private LodgingBookingId(uint value) => Value = value;

    public static Result<LodgingBookingId> Create(uint value)
    {
        if (value == 0)
            return Result<LodgingBookingId>.Failure("LodgingBookingId cannot be zero");
        return Result<LodgingBookingId>.Success(new LodgingBookingId(value));
    }

    public static implicit operator uint(LodgingBookingId id) => id.Value;
    public override string ToString() => Value.ToString();
}
