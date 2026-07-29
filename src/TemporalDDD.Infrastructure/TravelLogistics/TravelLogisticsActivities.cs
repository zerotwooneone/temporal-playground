using Temporalio.Activities;
using TemporalDDD.Application.TravelLogistics;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TravelLogistics.ValueObjects;

namespace TemporalDDD.Infrastructure.TravelLogistics;

public class TravelLogisticsActivities : ITravelLogisticsActivities
{
    public async Task<uint> BookFlightAsync(string flightNumber, string origin, string destination, DateTime departureTime, decimal cost)
    {
        // Simulate external API call to flight booking system
        await Task.Delay(1500);

        // Simulate occasional failure for testing compensating transactions
        if (flightNumber.Contains("FAIL"))
        {
            throw new Exception("Flight booking API unavailable");
        }

        // Create value objects
        var flightNumberVo = FlightNumber.Create(flightNumber);
        var originVo = AirportCode.Create(origin);
        var destinationVo = AirportCode.Create(destination);
        var departureTimeVo = FlightDepartureTime.Create(departureTime.ToUniversalTime());
        var costVo = Money.Create(cost);

        // Create domain entity using factory
        var booking = Domain.TravelLogistics.FlightBooking.Create(flightNumberVo, originVo, destinationVo, departureTimeVo, costVo);
        booking.Confirm();

        Console.WriteLine($"[FlightBooking] Booked flight {flightNumber} from {origin} to {destination} - ID: {booking.Id}");
        
        return booking.Id;
    }

    public async Task<uint> BookLodgingAsync(string hotelName, string address, DateTime checkInDate, DateTime checkOutDate, decimal cost)
    {
        // Simulate external API call to hotel booking system
        await Task.Delay(1500);

        // Simulate occasional failure for testing compensating transactions
        if (hotelName.Contains("FAIL"))
        {
            throw new Exception("Hotel booking API unavailable");
        }

        // Parse address components (simplified for demo)
        var addressParts = address.Split(',');
        var street = addressParts.Length > 0 ? addressParts[0].Trim() : "Unknown Street";
        var city = addressParts.Length > 1 ? addressParts[1].Trim() : "Unknown City";
        var state = addressParts.Length > 2 ? addressParts[2].Trim() : "Unknown State";
        var zipCode = addressParts.Length > 3 ? addressParts[3].Trim() : "00000";

        // Create value objects
        var hotelNameVo = HotelName.Create(hotelName);
        var addressVo = Address.Create(street, city, state, zipCode);
        var stayPeriodVo = DateRange.Create(checkInDate, checkOutDate);
        var costVo = Money.Create(cost);

        // Create domain entity using factory
        var booking = Domain.TravelLogistics.LodgingBooking.Create(hotelNameVo, addressVo, stayPeriodVo, costVo);
        booking.Confirm();

        Console.WriteLine($"[LodgingBooking] Booked hotel {hotelName} at {address} - ID: {booking.Id}");
        
        return booking.Id;
    }

    public async Task CancelFlightAsync(uint flightBookingId)
    {
        // Simulate external API call to cancel flight
        await Task.Delay(1000);

        // In real implementation, this would call the flight booking API to cancel
        Console.WriteLine($"[FlightCancellation] Cancelled flight booking {flightBookingId}");
    }

    public async Task CancelLodgingAsync(uint lodgingBookingId)
    {
        // Simulate external API call to cancel lodging
        await Task.Delay(1000);

        // In real implementation, this would call the hotel booking API to cancel
        Console.WriteLine($"[LodgingCancellation] Cancelled lodging booking {lodgingBookingId}");
    }

    public async Task NotifyTravelerAsync(string travelerEmail, string message, bool isCancellation)
    {
        // Simulate external notification (email/SMS)
        await Task.Delay(500);

        // In real implementation, this would send email or SMS via notification service
        var notificationType = isCancellation ? "CANCELLATION" : "CONFIRMATION";
        Console.WriteLine($"[TravelerNotification] {notificationType} sent to {travelerEmail}: {message}");
    }
}
