using Temporalio.Activities;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TravelLogistics;
using TemporalDDD.Domain.TravelLogistics.ValueObjects;

namespace TemporalDDD.Application.TravelLogistics;

public interface ITravelLogisticsActivities
{
    Task<FlightBookingId> BookFlightAsync(FlightNumber flightNumber, AirportCode origin, AirportCode destination, FlightDepartureTime departureTime, Money cost);
    Task<LodgingBookingId> BookLodgingAsync(HotelName hotelName, Address address, DateRange stayPeriod, Money cost);
    Task CancelFlightAsync(FlightBookingId flightBookingId);
    Task CancelLodgingAsync(LodgingBookingId lodgingBookingId);
    Task NotifyTravelerAsync(Email travelerEmail, string message, bool isCancellation);
}
