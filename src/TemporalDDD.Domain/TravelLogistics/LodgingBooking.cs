using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.TravelLogistics.ValueObjects;

namespace TemporalDDD.Domain.TravelLogistics;

public sealed class LodgingBooking
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
        // Cross-validation: Stay period cannot exceed 90 days (Long-Term Stay limit)
        if (stayPeriod.Days > 90)
            throw new ArgumentException("Lodging booking cannot exceed 90 days", nameof(stayPeriod));

        return new LodgingBooking
        {
            Id = LodgingBookingId.Create(0).Value!, // Temporary, will be set by DB
            PublicId = LodgingBookingPublicId.New(),
            HotelName = hotelName,
            Address = address,
            StayPeriod = stayPeriod,
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

    public void RecordNoShow()
    {
        if (Status != BookingStatus.Confirmed)
            throw new InvalidOperationException($"Cannot record no-show for booking in status: {Status}");

        Status = BookingStatus.NoShow;
    }

    public Money CalculateTotalCost()
    {
        // Cost is stored as rate per night, multiply by stay duration
        var totalNights = StayPeriod.Days;
        if (totalNights == 0)
            totalNights = 1; // Minimum 1 night

        return Money.Create(Cost.Amount * totalNights, Cost.Currency).Value ?? throw new InvalidOperationException("Failed to calculate total cost");
    }
}
