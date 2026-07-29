using Temporalio.Activities;
using TemporalDDD.Application.TravelLogistics;

namespace TemporalDDD.Infrastructure.TravelLogistics;

public class TravelLogisticsActivities : ITravelLogisticsActivities
{
    public async Task<Guid> BookFlightAsync(string flightNumber, string origin, string destination, DateTime departureTime, decimal cost)
    {
        // Simulate external API call to flight booking system
        await Task.Delay(1500);

        // Simulate occasional failure for testing compensating transactions
        if (flightNumber.Contains("FAIL"))
        {
            throw new Exception("Flight booking API unavailable");
        }

        // Create domain entity
        var booking = new Domain.TravelLogistics.FlightBooking(flightNumber, origin, destination, departureTime, cost);
        booking.Confirm();

        Console.WriteLine($"[FlightBooking] Booked flight {flightNumber} from {origin} to {destination} - ID: {booking.Id}");
        
        return booking.Id;
    }

    public async Task<Guid> BookLodgingAsync(string hotelName, string address, DateTime checkInDate, DateTime checkOutDate, decimal cost)
    {
        // Simulate external API call to hotel booking system
        await Task.Delay(1500);

        // Simulate occasional failure for testing compensating transactions
        if (hotelName.Contains("FAIL"))
        {
            throw new Exception("Hotel booking API unavailable");
        }

        // Create domain entity
        var booking = new Domain.TravelLogistics.LodgingBooking(hotelName, address, checkInDate, checkOutDate, cost);
        booking.Confirm();

        Console.WriteLine($"[LodgingBooking] Booked hotel {hotelName} at {address} - ID: {booking.Id}");
        
        return booking.Id;
    }

    public async Task CancelFlightAsync(Guid flightBookingId)
    {
        // Simulate external API call to cancel flight
        await Task.Delay(1000);

        // In real implementation, this would call the flight booking API to cancel
        Console.WriteLine($"[FlightCancellation] Cancelled flight booking {flightBookingId}");
    }

    public async Task CancelLodgingAsync(Guid lodgingBookingId)
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
