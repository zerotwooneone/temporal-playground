namespace TemporalDDD.Domain.TravelLogistics.ValueObjects;

public sealed record Address
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }

    private Address(string street, string city, string state, string zipCode)
    {
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
    }

    public static Address Create(string street, string city, string state, string zipCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new ArgumentException("Street cannot be null or whitespace", nameof(street));

        if (street.Trim().Length > 255)
            throw new ArgumentException("Street cannot exceed 255 characters", nameof(street));

        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be null or whitespace", nameof(city));

        if (city.Trim().Length > 255)
            throw new ArgumentException("City cannot exceed 255 characters", nameof(city));

        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be null or whitespace", nameof(state));

        if (state.Trim().Length > 50)
            throw new ArgumentException("State cannot exceed 50 characters", nameof(state));

        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("Zip code cannot be null or whitespace", nameof(zipCode));

        if (zipCode.Trim().Length > 50)
            throw new ArgumentException("Zip code cannot exceed 50 characters", nameof(zipCode));

        return new Address(street.Trim(), city.Trim(), state.Trim(), zipCode.Trim());
    }

    public override string ToString() => $"{Street}, {City}, {State} {ZipCode}";
}
