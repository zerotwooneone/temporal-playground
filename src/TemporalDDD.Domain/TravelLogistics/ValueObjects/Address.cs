using TemporalDDD.Domain.SharedKernel;

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

    public static Result<Address> Create(string street, string city, string state, string zipCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            return Result<Address>.Failure("Street cannot be null or whitespace");

        if (street.Trim().Length > 255)
            return Result<Address>.Failure("Street cannot exceed 255 characters");

        if (string.IsNullOrWhiteSpace(city))
            return Result<Address>.Failure("City cannot be null or whitespace");

        if (city.Trim().Length > 255)
            return Result<Address>.Failure("City cannot exceed 255 characters");

        if (string.IsNullOrWhiteSpace(state))
            return Result<Address>.Failure("State cannot be null or whitespace");

        if (state.Trim().Length > 50)
            return Result<Address>.Failure("State cannot exceed 50 characters");

        if (string.IsNullOrWhiteSpace(zipCode))
            return Result<Address>.Failure("Zip code cannot be null or whitespace");

        if (zipCode.Trim().Length > 50)
            return Result<Address>.Failure("Zip code cannot exceed 50 characters");

        return Result<Address>.Success(new Address(street.Trim(), city.Trim(), state.Trim(), zipCode.Trim()));
    }

    public override string ToString() => $"{Street}, {City}, {State} {ZipCode}";
}
