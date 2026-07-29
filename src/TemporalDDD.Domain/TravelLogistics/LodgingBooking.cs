using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TravelLogistics.ValueObjects;

namespace TemporalDDD.Domain.TravelLogistics;

public class LodgingBooking
{
    public LodgingBookingId Id { get; private set; }
    public LodgingBookingPublicId? PublicId { get; private set; }
    public HotelName HotelName { get; private set; }
    public Address Address { get; private set; }
    public DateRange StayPeriod { get; private set; }
    public Money Cost { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTimeOffset BookedAt { get; private set; }

    private LodgingBooking() { }

    // Factory for creating new booking (ID will be set by database)
    public static LodgingBooking Create(HotelName hotelName, Address address, DateRange stayPeriod, Money cost)
    {
        return new LodgingBooking
        {
            Id = LodgingBookingId.Create(0), // Temporary, will be set by DB
            PublicId = LodgingBookingPublicId.New(),
            HotelName = hotelName,
            Address = address,
            StayPeriod = stayPeriod,
            Cost = cost,
            Status = BookingStatus.Pending,
            BookedAt = DateTimeOffset.UtcNow
        };
    }

    // Factory for rehydrating from database
    public static LodgingBooking FromDatabase(uint id, Guid? publicId, HotelName hotelName, Address address, DateRange stayPeriod, Money cost, BookingStatus status, DateTimeOffset bookedAt)
    {
        return new LodgingBooking
        {
            Id = LodgingBookingId.FromDatabase(id),
            PublicId = publicId.HasValue ? LodgingBookingPublicId.Create(publicId.Value) : null,
            HotelName = hotelName,
            Address = address,
            StayPeriod = stayPeriod,
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
