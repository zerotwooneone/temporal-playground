using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TravelLogistics.ValueObjects;

namespace TemporalDDD.Domain.TravelLogistics;

public class FlightBooking
{
    public FlightBookingId Id { get; private set; }
    public FlightBookingPublicId? PublicId { get; private set; }
    public FlightNumber FlightNumber { get; private set; }
    public AirportCode Origin { get; private set; }
    public AirportCode Destination { get; private set; }
    public FlightDepartureTime DepartureTime { get; private set; }
    public Money Cost { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTimeOffset BookedAt { get; private set; }

    private FlightBooking() { }

    // Factory for creating new booking (ID will be set by database)
    public static FlightBooking Create(FlightNumber flightNumber, AirportCode origin, AirportCode destination, FlightDepartureTime departureTime, Money cost)
    {
        return new FlightBooking
        {
            Id = FlightBookingId.Create(0), // Temporary, will be set by DB
            PublicId = FlightBookingPublicId.New(),
            FlightNumber = flightNumber,
            Origin = origin,
            Destination = destination,
            DepartureTime = departureTime,
            Cost = cost,
            Status = BookingStatus.Pending,
            BookedAt = DateTimeOffset.UtcNow
        };
    }

    // Factory for rehydrating from database
    public static FlightBooking FromDatabase(uint id, Guid? publicId, FlightNumber flightNumber, AirportCode origin, AirportCode destination, FlightDepartureTime departureTime, Money cost, BookingStatus status, DateTimeOffset bookedAt)
    {
        return new FlightBooking
        {
            Id = FlightBookingId.FromDatabase(id),
            PublicId = publicId.HasValue ? FlightBookingPublicId.Create(publicId.Value) : null,
            FlightNumber = flightNumber,
            Origin = origin,
            Destination = destination,
            DepartureTime = departureTime,
            Cost = cost,
            Status = status,
            BookedAt = bookedAt
        };
    }

    public void Confirm()
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm booking in status: {Status}");

        Status = BookingStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Booking is already cancelled");

        Status = BookingStatus.Cancelled;
    }
}
