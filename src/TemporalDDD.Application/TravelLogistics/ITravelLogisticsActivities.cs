using Temporalio.Activities;

namespace TemporalDDD.Application.TravelLogistics;

public interface ITravelLogisticsActivities
{
    Task<Guid> BookFlightAsync(string flightNumber, string origin, string destination, DateTime departureTime, decimal cost);
    Task<Guid> BookLodgingAsync(string hotelName, string address, DateTime checkInDate, DateTime checkOutDate, decimal cost);
    Task CancelFlightAsync(Guid flightBookingId);
    Task CancelLodgingAsync(Guid lodgingBookingId);
    Task NotifyTravelerAsync(string travelerEmail, string message, bool isCancellation);
}
