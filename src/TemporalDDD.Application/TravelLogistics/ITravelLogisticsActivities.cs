using Temporalio.Activities;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TravelLogistics;
using TemporalDDD.Domain.TravelLogistics.ValueObjects;

namespace TemporalDDD.Application.TravelLogistics;

public interface ITravelLogisticsActivities
{
    [Activity]
    Task<FlightBookingId> BookFlightAsync(FlightNumber flightNumber, AirportCode origin, AirportCode destination, FlightDepartureTime departureTime, Money cost);
    [Activity]
    Task<LodgingBookingId> BookLodgingAsync(HotelName hotelName, Address address, DateRange stayPeriod, Money cost);
    [Activity]
    Task CancelFlightAsync(FlightBookingId flightBookingId);
    [Activity]
    Task CancelLodgingAsync(LodgingBookingId lodgingBookingId);
    [Activity]
    Task NotifyTravelerAsync(Email travelerEmail, string message, bool isCancellation);
}
