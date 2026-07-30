using System.Text.RegularExpressions;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.TravelLogistics.ValueObjects;

public sealed record FlightNumber
{
    private static readonly Regex FlightNumberRegex = new(@"^[A-Z]{2}\d{1,4}$", RegexOptions.Compiled);

    public string Value { get; }

    private FlightNumber(string value)
    {
        Value = value;
    }

    public static Result<FlightNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<FlightNumber>.Failure("Flight number cannot be null or whitespace");

        var trimmed = value.Trim().ToUpperInvariant();

        if (!FlightNumberRegex.IsMatch(trimmed))
            return Result<FlightNumber>.Failure("Flight number must be in IATA format: 2 uppercase letters followed by 1-4 digits (e.g., UA1234)");

        return Result<FlightNumber>.Success(new FlightNumber(trimmed));
    }

    public static implicit operator string(FlightNumber flightNumber) => flightNumber.Value;

    public override string ToString() => Value;
}
