using System.Text.RegularExpressions;

namespace TemporalDDD.Domain.TravelLogistics.ValueObjects;

public sealed record HotelName
{
    private static readonly Regex HotelNameRegex = new(@"^[a-zA-Z0-9\s\-']+$", RegexOptions.Compiled);

    public string Value { get; }

    private HotelName(string value)
    {
        Value = value;
    }

    public static HotelName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Hotel name cannot be null or whitespace", nameof(value));

        var trimmed = value.Trim();

        if (trimmed.Length < 2 || trimmed.Length > 255)
            throw new ArgumentException("Hotel name must be between 2 and 255 characters", nameof(value));

        if (!HotelNameRegex.IsMatch(trimmed))
            throw new ArgumentException("Hotel name must contain only letters, numbers, spaces, hyphens, and apostrophes", nameof(value));

        return new HotelName(trimmed);
    }

    public static implicit operator string(HotelName hotelName) => hotelName.Value;

    public override string ToString() => Value;
}
