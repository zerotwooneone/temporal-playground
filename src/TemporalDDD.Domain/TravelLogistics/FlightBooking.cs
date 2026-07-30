using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TravelLogistics.ValueObjects;

namespace TemporalDDD.Domain.TravelLogistics;

public sealed class FlightBooking
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

    internal FlightBooking() { }

    // Internal constructor for infrastructure rehydration
    internal FlightBooking(FlightBookingId id, FlightBookingPublicId? publicId, FlightNumber flightNumber, AirportCode origin, AirportCode destination, FlightDepartureTime departureTime, Money cost, BookingStatus status, DateTimeOffset bookedAt)
    {
        Id = id;
        PublicId = publicId;
        FlightNumber = flightNumber;
        Origin = origin;
        Destination = destination;
        DepartureTime = departureTime;
        Cost = cost;
        Status = status;
        BookedAt = bookedAt;
    }

    // Factory for creating new booking (ID will be set by database)
    public static FlightBooking Create(FlightNumber flightNumber, AirportCode origin, AirportCode destination, FlightDepartureTime departureTime, Money cost)
    {
        // Cross-validation: Origin and Destination must be different
        if (origin.Value == destination.Value)
            throw new ArgumentException("Origin and destination airport codes cannot be the same", nameof(destination));

        return new FlightBooking
        {
            Id = FlightBookingId.Create(0).Value!, // Temporary, will be set by DB
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

    

    public void Confirm()
    {
        if (Status != BookingStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm booking in status: {Status}");

        Status = BookingStatus.Confirmed;
    }

    public void MarkAsCancelled()
    {
        if (Status == BookingStatus.Cancelled)
            throw new InvalidOperationException("Booking is already cancelled");

        Status = BookingStatus.Cancelled;
    }

    public Money CalculateRefundAmount(DateTimeOffset cancellationRequestDate)
    {
        if (Status != BookingStatus.Confirmed && Status != BookingStatus.Pending)
            return Money.Zero(Cost.Currency);

        var departureTime = DepartureTime.Value;
        var timeUntilDeparture = departureTime - cancellationRequestDate;

        // 100% refund if cancelled more than 14 days before departure
        if (timeUntilDeparture.TotalDays > 14)
            return Cost;

        // 50% refund if cancelled within 14 days but more than 24 hours before departure
        if (timeUntilDeparture.TotalDays > 1)
            return Money.Create(Cost.Amount * 0.5m, Cost.Currency).Value ?? throw new InvalidOperationException("Failed to calculate refund amount");

        // 0% refund if cancelled within 24 hours of departure
        return Money.Zero(Cost.Currency);
    }
}
