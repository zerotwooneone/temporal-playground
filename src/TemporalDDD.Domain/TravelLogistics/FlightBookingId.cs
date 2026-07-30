using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.TravelLogistics;

public sealed record FlightBookingId
{
    private const string Abbreviation = "FLT";
    public Guid Value { get; }

    private FlightBookingId(Guid value) => Value = value;

    public static Result<FlightBookingId> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Result<FlightBookingId>.Failure("FlightBooking ID cannot be null or empty");

        var expectedPrefix = $"{Abbreviation}Id";
        if (!value.StartsWith(expectedPrefix))
            return Result<FlightBookingId>.Failure($"FlightBooking ID must start with '{expectedPrefix}'");

        var guidString = value.Substring(expectedPrefix.Length);
        if (!Guid.TryParse(guidString, out var guid))
            return Result<FlightBookingId>.Failure("Invalid GUID format in FlightBooking ID");

        return Result<FlightBookingId>.Success(new FlightBookingId(guid));
    }

    public static FlightBookingId New() => new FlightBookingId(Guid.CreateVersion7());

    public override string ToString() => $"{Abbreviation}Id{Value}";
}
