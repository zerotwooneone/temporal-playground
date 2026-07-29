using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TravelLogistics.ValueObjects;

namespace TemporalDDD.Domain.TravelLogistics;

public class LodgingBooking
{
    public Guid Id { get; private set; }
    public HotelName HotelName { get; private set; }
    public Address Address { get; private set; }
    public DateRange StayPeriod { get; private set; }
    public Money Cost { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime BookedAt { get; private set; }

    private LodgingBooking() { }

    public LodgingBooking(HotelName hotelName, Address address, DateRange stayPeriod, Money cost)
    {
        Id = Guid.NewGuid();
        HotelName = hotelName;
        Address = address;
        StayPeriod = stayPeriod;
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
