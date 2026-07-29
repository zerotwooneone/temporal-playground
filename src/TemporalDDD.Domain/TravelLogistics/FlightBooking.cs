namespace TemporalDDD.Domain.TravelLogistics;

public class FlightBooking
{
    public Guid Id { get; private set; }
    public string FlightNumber { get; private set; }
    public string Origin { get; private set; }
    public string Destination { get; private set; }
    public DateTime DepartureTime { get; private set; }
    public decimal Cost { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime BookedAt { get; private set; }

    private FlightBooking() { }

    public FlightBooking(string flightNumber, string origin, string destination, DateTime departureTime, decimal cost)
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

public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled
}
