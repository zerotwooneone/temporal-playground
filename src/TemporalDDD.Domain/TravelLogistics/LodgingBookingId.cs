using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.TravelLogistics;

public sealed record LodgingBookingId
{
    private const string Abbreviation = "LDG";
    public Guid Value { get; }

    private LodgingBookingId(Guid value) => Value = value;

    public static Result<LodgingBookingId> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Result<LodgingBookingId>.Failure("LodgingBooking ID cannot be null or empty");

        var expectedPrefix = $"{Abbreviation}Id";
        if (!value.StartsWith(expectedPrefix))
            return Result<LodgingBookingId>.Failure($"LodgingBooking ID must start with '{expectedPrefix}'");

        var guidString = value.Substring(expectedPrefix.Length);
        if (!Guid.TryParse(guidString, out var guid))
            return Result<LodgingBookingId>.Failure("Invalid GUID format in LodgingBooking ID");

        return Result<LodgingBookingId>.Success(new LodgingBookingId(guid));
    }

    public static LodgingBookingId New() => new LodgingBookingId(Guid.CreateVersion7());

    public override string ToString() => $"{Abbreviation}Id{Value}";
}
