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

    public async Task<uint> BookFlightAsync(string flightNumber, string origin, string destination, DateTime departureTime, decimal cost)
    {
        // Simulate external API call to flight booking system with chaos (100ms latency, 10% failure rate)
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        var response = await _chaosHttpClient.PostAsJsonAsync($"/api/flights/book", new { FlightNumber = flightNumber, Origin = origin, Destination = destination, DepartureTime = departureTime, Cost = cost });

        // Create value objects
        var flightNumberVo = FlightNumber.Create(flightNumber);
        var originVo = AirportCode.Create(origin);
        var destinationVo = AirportCode.Create(destination);
        var departureTimeVo = FlightDepartureTime.Create(departureTime.ToUniversalTime());
        var costVo = Money.Create(cost);

        // Create domain entity using factory
        var booking = Domain.TravelLogistics.FlightBooking.Create(flightNumberVo, originVo, destinationVo, departureTimeVo, costVo);
        booking.Confirm();

        await _flightBookingRepository.SaveAsync(booking);

        Console.WriteLine($"[FlightBooking] Booked flight {flightNumber} from {origin} to {destination} - ID: {booking.Id}");
        
        return booking.Id;
    }

    public async Task<uint> BookLodgingAsync(string hotelName, string address, DateTime checkInDate, DateTime checkOutDate, decimal cost)
    {
        // Simulate external API call to hotel booking system with chaos (100ms latency, 10% failure rate)
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        var response = await _chaosHttpClient.PostAsJsonAsync($"/api/hotels/book", new { HotelName = hotelName, Address = address, CheckInDate = checkInDate, CheckOutDate = checkOutDate, Cost = cost });

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

        await _lodgingBookingRepository.SaveAsync(booking);

        Console.WriteLine($"[LodgingBooking] Booked hotel {hotelName} at {address} - ID: {booking.Id}");
        
        return booking.Id;
    }

    public async Task CancelFlightAsync(uint flightBookingId)
    {
        var flightBookingIdVo = FlightBookingId.Create(flightBookingId);
        var booking = await _flightBookingRepository.GetByIdAsync(flightBookingIdVo);

        if (booking == null)
        {
            throw new InvalidOperationException($"Flight booking {flightBookingId} not found");
        }

        booking.MarkAsCancelled();
        await _flightBookingRepository.SaveAsync(booking);

        // Simulate external API call to cancel flight with chaos (100ms latency, 10% failure rate)
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        await _chaosHttpClient.PostAsJsonAsync($"/api/flights/cancel/{flightBookingId}", new { });

        Console.WriteLine($"[FlightCancellation] Cancelled flight booking {flightBookingId}");
    }

    public async Task CancelLodgingAsync(uint lodgingBookingId)
    {
        var lodgingBookingIdVo = LodgingBookingId.Create(lodgingBookingId);
        var booking = await _lodgingBookingRepository.GetByIdAsync(lodgingBookingIdVo);

        if (booking == null)
        {
            throw new InvalidOperationException($"Lodging booking {lodgingBookingId} not found");
        }

        booking.MarkAsCancelled();
        await _lodgingBookingRepository.SaveAsync(booking);

        // Simulate external API call to cancel lodging with chaos (100ms latency, 10% failure rate)
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        await _chaosHttpClient.PostAsJsonAsync($"/api/hotels/cancel/{lodgingBookingId}", new { });

        Console.WriteLine($"[LodgingCancellation] Cancelled lodging booking {lodgingBookingId}");
    }

    public async Task NotifyTravelerAsync(string travelerEmail, string message, bool isCancellation)
    {
        // Simulate external notification (email/SMS) with chaos (100ms latency, 10% failure rate)
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        var notificationType = isCancellation ? "cancellation" : "confirmation";
        await _chaosHttpClient.PostAsJsonAsync($"/api/notifications/{notificationType}", new { Email = travelerEmail, Message = message });

        var notificationTypeDisplay = isCancellation ? "CANCELLATION" : "CONFIRMATION";
        Console.WriteLine($"[TravelerNotification] {notificationTypeDisplay} sent to {travelerEmail}: {message}");
    }
}
