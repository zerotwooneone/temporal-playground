using Temporalio.Activities;
using TemporalDDD.Application.TravelLogistics;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TravelLogistics;
using TemporalDDD.Domain.TravelLogistics.ValueObjects;
using TemporalDDD.Infrastructure.Testing;

namespace TemporalDDD.Infrastructure.TravelLogistics;

public class TravelLogisticsActivities : ITravelLogisticsActivities
{
    private readonly IFlightBookingRepository _flightBookingRepository;
    private readonly ILodgingBookingRepository _lodgingBookingRepository;
    private readonly ChaosHttpClient _chaosHttpClient;

    public TravelLogisticsActivities(
        IFlightBookingRepository flightBookingRepository,
        ILodgingBookingRepository lodgingBookingRepository,
        ChaosHttpClient chaosHttpClient)
    {
        _flightBookingRepository = flightBookingRepository;
        _lodgingBookingRepository = lodgingBookingRepository;
        _chaosHttpClient = chaosHttpClient;
    }

    [Activity]
    public async Task<FlightBookingId> BookFlightAsync(FlightNumber flightNumber, AirportCode origin, AirportCode destination, FlightDepartureTime departureTime, Money cost)
    {
        // Simulate external API call to flight booking system with chaos (100ms latency, 10% failure rate)
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        var response = await _chaosHttpClient.PostAsJsonAsync("https://airline-api.example.com/api/bookings", new { FlightNumber = flightNumber.Value, Origin = origin.Value, Destination = destination.Value, DepartureTime = departureTime.Value, Cost = cost.Amount });

        // Create domain entity using factory
        var booking = Domain.TravelLogistics.FlightBooking.Create(flightNumber, origin, destination, departureTime, cost);
        booking.Confirm();

        await _flightBookingRepository.SaveAsync(booking);

        Console.WriteLine($"[FlightBooking] Booked flight {flightNumber.Value} from {origin.Value} to {destination.Value} - ID: {booking.Id.Value}");
        
        return booking.Id;
    }

    [Activity]
    public async Task<LodgingBookingId> BookLodgingAsync(HotelName hotelName, Address address, DateRange stayPeriod, Money cost)
    {
        // Simulate external API call to hotel booking system with chaos (100ms latency, 10% failure rate)
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        var response = await _chaosHttpClient.PostAsJsonAsync("https://hotel-booking.example.com/api/reservations", new { HotelName = hotelName.Value, Address = address.ToString(), CheckInDate = stayPeriod.Start, CheckOutDate = stayPeriod.End, Cost = cost.Amount });

        // Create domain entity using factory
        var booking = Domain.TravelLogistics.LodgingBooking.Create(hotelName, address, stayPeriod, cost);
        booking.Confirm();

        await _lodgingBookingRepository.SaveAsync(booking);

        Console.WriteLine($"[LodgingBooking] Booked hotel {hotelName.Value} at {address.ToString()} - ID: {booking.Id.Value}");
        
        return booking.Id;
    }

    [Activity]
    public async Task CancelFlightAsync(FlightBookingId flightBookingId)
    {
        var booking = await _flightBookingRepository.GetByIdAsync(flightBookingId);

        if (booking == null)
        {
            throw new InvalidOperationException($"Flight booking {flightBookingId.Value} not found");
        }

        booking.MarkAsCancelled();
        await _flightBookingRepository.SaveAsync(booking);

        // Simulate external API call to cancel flight with chaos (100ms latency, 10% failure rate)
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        await _chaosHttpClient.PostAsJsonAsync($"https://airline-api.example.com/api/bookings/{flightBookingId.Value}/cancel", new { });

        Console.WriteLine($"[FlightCancellation] Cancelled flight booking {flightBookingId.Value}");
    }

    [Activity]
    public async Task CancelLodgingAsync(LodgingBookingId lodgingBookingId)
    {
        var booking = await _lodgingBookingRepository.GetByIdAsync(lodgingBookingId);

        if (booking == null)
        {
            throw new InvalidOperationException($"Lodging booking {lodgingBookingId.Value} not found");
        }

        booking.MarkAsCancelled();
        await _lodgingBookingRepository.SaveAsync(booking);

        // Simulate external API call to cancel lodging with chaos (100ms latency, 10% failure rate)
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        await _chaosHttpClient.PostAsJsonAsync($"https://hotel-booking.example.com/api/reservations/{lodgingBookingId.Value}/cancel", new { });

        Console.WriteLine($"[LodgingCancellation] Cancelled lodging booking {lodgingBookingId.Value}");
    }

    [Activity]
    public async Task NotifyTravelerAsync(Email travelerEmail, string message, bool isCancellation)
    {
        // Simulate external notification (email/SMS) with chaos (100ms latency, 10% failure rate)
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        var notificationType = isCancellation ? "cancellation" : "confirmation";
        await _chaosHttpClient.PostAsJsonAsync($"https://notifications.example.com/api/{notificationType}", new { Email = travelerEmail.Value, Message = message });

        var notificationTypeDisplay = isCancellation ? "CANCELLATION" : "CONFIRMATION";
        Console.WriteLine($"[TravelerNotification] {notificationTypeDisplay} sent to {travelerEmail.Value}: {message}");
    }
}
