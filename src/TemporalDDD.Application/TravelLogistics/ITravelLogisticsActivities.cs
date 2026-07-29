using Temporalio.Activities;

namespace TemporalDDD.Application.TravelLogistics;

public interface ITravelLogisticsActivities
{
    Task<uint> BookFlightAsync(string flightNumber, string origin, string destination, DateTime departureTime, decimal cost);
    Task<uint> BookLodgingAsync(string hotelName, string address, DateTime checkInDate, DateTime checkOutDate, decimal cost);
    Task CancelFlightAsync(uint flightBookingId);
    Task CancelLodgingAsync(uint lodgingBookingId);
    Task NotifyTravelerAsync(string travelerEmail, string message, bool isCancellation);
}
