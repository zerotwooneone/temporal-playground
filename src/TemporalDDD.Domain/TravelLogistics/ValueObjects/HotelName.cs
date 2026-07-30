using System.Text.RegularExpressions;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.TravelLogistics.ValueObjects;

public sealed record HotelName
{
    private static readonly Regex HotelNameRegex = new(@"^[a-zA-Z0-9\s\-']+$", RegexOptions.Compiled);

    public string Value { get; }

    private HotelName(string value)
    {
        Value = value;
    }

    public static Result<HotelName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<HotelName>.Failure("Hotel name cannot be null or whitespace");

        var trimmed = value.Trim();

        if (trimmed.Length < 2 || trimmed.Length > 255)
            return Result<HotelName>.Failure("Hotel name must be between 2 and 255 characters");

        if (!HotelNameRegex.IsMatch(trimmed))
            return Result<HotelName>.Failure("Hotel name must contain only letters, numbers, spaces, hyphens, and apostrophes");

        return Result<HotelName>.Success(new HotelName(trimmed));
    }

    public static implicit operator string(HotelName hotelName) => hotelName.Value;

    public override string ToString() => Value;
}
