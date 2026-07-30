using Temporalio.Activities;

namespace TemporalDDD.Application.TravelLogistics;

public interface ITravelLogisticsActivities
{
    [Activity]
    Task<uint> BookFlightAsync(BookFlightInput input);
    [Activity]
    Task<uint> BookLodgingAsync(BookLodgingInput input);
    [Activity]
    Task CancelFlightAsync(CancelFlightInput input);
    [Activity]
    Task CancelLodgingAsync(CancelLodgingInput input);
    [Activity]
    Task NotifyTravelerAsync(NotifyTravelerInput input);
}

// Primitive DTOs for activity parameters
public record BookFlightInput(string FlightNumber, string Origin, string Destination, DateTimeOffset DepartureTime, decimal Cost);
public record BookLodgingInput(string HotelName, string Street, string City, string State, string ZipCode, DateTimeOffset CheckInDate, DateTimeOffset CheckOutDate, decimal Cost);
public record CancelFlightInput(uint FlightBookingId);
public record CancelLodgingInput(uint LodgingBookingId);
public record NotifyTravelerInput(string TravelerEmail, string Message, bool IsCancellation);
