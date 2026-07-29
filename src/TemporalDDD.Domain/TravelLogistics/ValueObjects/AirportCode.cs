using System.Text.RegularExpressions;

namespace TemporalDDD.Domain.TravelLogistics.ValueObjects;

public sealed record AirportCode
{
    private static readonly Regex AirportCodeRegex = new(@"^[A-Z]{3}$", RegexOptions.Compiled);

    public string Value { get; }

    private AirportCode(string value)
    {
        Value = value;
    }

    public static AirportCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Airport code cannot be null or whitespace", nameof(value));

        var trimmed = value.Trim().ToUpperInvariant();

        if (!AirportCodeRegex.IsMatch(trimmed))
            throw new ArgumentException("Airport code must be exactly 3 uppercase letters (IATA code)", nameof(value));

        return new AirportCode(trimmed);
    }

    public static implicit operator string(AirportCode airportCode) => airportCode.Value;

    public override string ToString() => Value;
}
