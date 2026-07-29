using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TravelLogistics.ValueObjects;

namespace TemporalDDD.Domain.TravelLogistics;

public class FlightBooking
{
    public Guid Id { get; private set; }
    public FlightNumber FlightNumber { get; private set; }
    public AirportCode Origin { get; private set; }
    public AirportCode Destination { get; private set; }
    public FlightDepartureTime DepartureTime { get; private set; }
    public Money Cost { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime BookedAt { get; private set; }

    private FlightBooking() { }

    public FlightBooking(FlightNumber flightNumber, AirportCode origin, AirportCode destination, FlightDepartureTime departureTime, Money cost)
    {
        Id = Guid.NewGuid();
        FlightNumber = flightNumber;
        Origin = origin;
        Destination = destination;
        DepartureTime = departureTime;
        Cost = cost;
        Status = BookingStatus.Pending;
        BookedAt = DateTime.UtcNow;
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
