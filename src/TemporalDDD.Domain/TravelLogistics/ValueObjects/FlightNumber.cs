using System.Text.RegularExpressions;

namespace TemporalDDD.Domain.TravelLogistics.ValueObjects;

public sealed record FlightNumber
{
    private static readonly Regex FlightNumberRegex = new(@"^[A-Z]{2}\d{1,4}$", RegexOptions.Compiled);

    public string Value { get; }

    private FlightNumber(string value)
    {
        Value = value;
    }

    public static FlightNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Flight number cannot be null or whitespace", nameof(value));

        var trimmed = value.Trim().ToUpperInvariant();

        if (!FlightNumberRegex.IsMatch(trimmed))
            throw new ArgumentException("Flight number must be in IATA format: 2 uppercase letters followed by 1-4 digits (e.g., UA1234)", nameof(value));

        return new FlightNumber(trimmed);
    }

    public static implicit operator string(FlightNumber flightNumber) => flightNumber.Value;

    public override string ToString() => Value;
}
