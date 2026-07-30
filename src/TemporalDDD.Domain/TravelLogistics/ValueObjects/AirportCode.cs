using System.Text.RegularExpressions;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.TravelLogistics.ValueObjects;

public sealed record AirportCode
{
    private static readonly Regex AirportCodeRegex = new(@"^[A-Z]{3}$", RegexOptions.Compiled);

    public string Value { get; }

    private AirportCode(string value)
    {
        Value = value;
    }

    public static Result<AirportCode> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<AirportCode>.Failure("Airport code cannot be null or whitespace");

        var trimmed = value.Trim().ToUpperInvariant();

        if (!AirportCodeRegex.IsMatch(trimmed))
            return Result<AirportCode>.Failure("Airport code must be exactly 3 uppercase letters (IATA code)");

        return Result<AirportCode>.Success(new AirportCode(trimmed));
    }

    public static implicit operator string(AirportCode airportCode) => airportCode.Value;

    public override string ToString() => Value;
}
