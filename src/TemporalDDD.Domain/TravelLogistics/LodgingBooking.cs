namespace TemporalDDD.Domain.TravelLogistics;

public class LodgingBooking
{
    public Guid Id { get; private set; }
    public string HotelName { get; private set; }
    public string Address { get; private set; }
    public DateTime CheckInDate { get; private set; }
    public DateTime CheckOutDate { get; private set; }
    public decimal Cost { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime BookedAt { get; private set; }

    private LodgingBooking() { }

    public LodgingBooking(string hotelName, string address, DateTime checkInDate, DateTime checkOutDate, decimal cost)
    {
        Id = Guid.NewGuid();
        HotelName = hotelName;
        Address = address;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
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
