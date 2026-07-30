namespace TemporalDDD.Application.TravelLogistics;

/// <summary>
/// Primitive DTO for Travel Logistics Saga Workflow input.
/// All fields are primitive types to ensure clean JSON serialization with Temporal.
/// </summary>
public record TravelLogisticsInput(
    string TravelerEmail,
    string FlightNumber,
    string OriginAirportCode,
    string DestinationAirportCode,
    DateTimeOffset DepartureTimeUtc,
    decimal FlightCostAmount,
    string FlightCostCurrency,
    string HotelName,
    string AddressStreet,
    string AddressCity,
    string AddressState,
    string AddressZipCode,
    DateTimeOffset StayPeriodStartUtc,
    DateTimeOffset StayPeriodEndUtc,
    decimal LodgingCostAmount,
    string LodgingCostCurrency
);
