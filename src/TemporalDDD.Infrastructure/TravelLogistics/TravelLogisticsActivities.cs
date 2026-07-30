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
    public async Task<uint> BookFlightAsync(BookFlightInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var flightNumberResult = FlightNumber.Create(input.FlightNumber);
        if (flightNumberResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid FlightNumber. {flightNumberResult.Error}");
        
        var originResult = AirportCode.Create(input.Origin);
        if (originResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid Origin. {originResult.Error}");
        
        var destinationResult = AirportCode.Create(input.Destination);
        if (destinationResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid Destination. {destinationResult.Error}");
        
        var departureTimeResult = FlightDepartureTime.Create(input.DepartureTime);
        if (departureTimeResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid DepartureTime. {departureTimeResult.Error}");
        
        var costResult = Money.Create(input.Cost);
        if (costResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid Cost. {costResult.Error}");

        var flightNumber = flightNumberResult.Value;
        var origin = originResult.Value;
        var destination = destinationResult.Value;
        var departureTime = departureTimeResult.Value;
        var cost = costResult.Value;

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
        
        return booking.Id.Value;
    }

    [Activity]
    public async Task<uint> BookLodgingAsync(BookLodgingInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var hotelNameResult = HotelName.Create(input.HotelName);
        if (hotelNameResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid HotelName. {hotelNameResult.Error}");
        
        var addressResult = Address.Create(input.Street, input.City, input.State, input.ZipCode);
        if (addressResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid Address. {addressResult.Error}");
        
        var stayPeriodResult = DateRange.Create(input.CheckInDate, input.CheckOutDate);
        if (stayPeriodResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid StayPeriod. {stayPeriodResult.Error}");
        
        var costResult = Money.Create(input.Cost);
        if (costResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid Cost. {costResult.Error}");

        var hotelName = hotelNameResult.Value;
        var address = addressResult.Value;
        var stayPeriod = stayPeriodResult.Value;
        var cost = costResult.Value;

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
        
        return booking.Id.Value;
    }

    [Activity]
    public async Task CancelFlightAsync(CancelFlightInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var flightBookingIdResult = FlightBookingId.Create(input.FlightBookingId);
        if (flightBookingIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid FlightBookingId. {flightBookingIdResult.Error}");

        var flightBookingId = flightBookingIdResult.Value;
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
    public async Task CancelLodgingAsync(CancelLodgingInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var lodgingBookingIdResult = LodgingBookingId.Create(input.LodgingBookingId);
        if (lodgingBookingIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid LodgingBookingId. {lodgingBookingIdResult.Error}");

        var lodgingBookingId = lodgingBookingIdResult.Value;
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
    public async Task NotifyTravelerAsync(NotifyTravelerInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var travelerEmailResult = Email.Create(input.TravelerEmail);
        if (travelerEmailResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid TravelerEmail. {travelerEmailResult.Error}");

        var travelerEmail = travelerEmailResult.Value;

        // Simulate external notification (email/SMS) with chaos (100ms latency, 10% failure rate)
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        var notificationType = input.IsCancellation ? "cancellation" : "confirmation";
        await _chaosHttpClient.PostAsJsonAsync($"https://notifications.example.com/api/{notificationType}", new { Email = travelerEmail.Value, Message = input.Message });

        var notificationTypeDisplay = input.IsCancellation ? "CANCELLATION" : "CONFIRMATION";
        Console.WriteLine($"[TravelerNotification] {notificationTypeDisplay} sent to {travelerEmail.Value}: {input.Message}");
    }
}
