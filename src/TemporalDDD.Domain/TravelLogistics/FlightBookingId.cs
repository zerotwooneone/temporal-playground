using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.TravelLogistics;

public sealed record FlightBookingId
{
    public uint Value { get; }

    private FlightBookingId(uint value) => Value = value;

    public static Result<FlightBookingId> Create(uint value)
    {
        if (value == 0)
            return Result<FlightBookingId>.Failure("FlightBookingId cannot be zero");
        return Result<FlightBookingId>.Success(new FlightBookingId(value));
    }

    public static implicit operator uint(FlightBookingId id) => id.Value;
    public override string ToString() => Value.ToString();
}
