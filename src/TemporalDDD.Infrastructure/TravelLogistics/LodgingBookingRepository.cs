using Microsoft.EntityFrameworkCore;
using TemporalDDD.Domain.TravelLogistics;
using TemporalDDD.Domain.TravelLogistics.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.TravelLogistics;

public class LodgingBookingRepository : ILodgingBookingRepository
{
    private readonly ApplicationDbContext _dbContext;

    public LodgingBookingRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<LodgingBooking?> GetByIdAsync(LodgingBookingId id, CancellationToken cancellationToken = default)
    {
        var dbo = await _dbContext.LodgingBookings
            .FirstOrDefaultAsync(l => l.Id == id.Value, cancellationToken);
        
        if (dbo == null) return null;
        
        return MapToDomain(dbo);
    }

    public async Task SaveAsync(LodgingBooking aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.LodgingBookings
            .FirstOrDefaultAsync(l => l.Id == aggregate.Id.Value, cancellationToken);

        var dbo = MapToDbo(aggregate);

        if (existing == null)
        {
            _dbContext.LodgingBookings.Add(dbo);
        }
        else
        {
            _dbContext.Entry(existing).State = EntityState.Detached;
            _dbContext.LodgingBookings.Attach(dbo);
            _dbContext.Entry(dbo).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private LodgingBooking MapToDomain(LodgingBookingDbo dbo)
    {
        var hotelName = HotelName.Create(dbo.HotelName).Value;
        var address = Address.Create(dbo.AddressStreet, dbo.AddressCity, dbo.AddressState, dbo.AddressZipCode);
        var stayPeriodStart = DateTimeOffset.FromUnixTimeMilliseconds(dbo.StayPeriodStartUtc);
        var stayPeriodEnd = DateTimeOffset.FromUnixTimeMilliseconds(dbo.StayPeriodEndUtc);
        var stayPeriod = DateRange.Create(stayPeriodStart, stayPeriodEnd);
        var cost = Money.Create(decimal.Parse(dbo.CostAmount), dbo.CostCurrency);
        var status = BookingStatus.FromValue(dbo.Status);
        var bookedAt = DateTimeOffset.FromUnixTimeMilliseconds(dbo.BookedAt);

        LodgingBookingPublicId? publicId = null;
        if (!string.IsNullOrEmpty(dbo.PublicId))
        {
            publicId = LodgingBookingPublicId.FromString(dbo.PublicId);
        }

        // Use reflection to call private constructor for rehydration
        var booking = (LodgingBooking)Activator.CreateInstance(
            typeof(LodgingBooking),
            nonPublic: true)!;
        
        // Set properties via reflection (infrastructure concern)
        typeof(LodgingBooking).GetProperty(nameof(LodgingBooking.Id))?.SetValue(booking, LodgingBookingId.Create(dbo.Id).Value);
        typeof(LodgingBooking).GetProperty(nameof(LodgingBooking.PublicId))?.SetValue(booking, publicId);
        typeof(LodgingBooking).GetProperty(nameof(LodgingBooking.HotelName))?.SetValue(booking, hotelName);
        typeof(LodgingBooking).GetProperty(nameof(LodgingBooking.Address))?.SetValue(booking, address);
        typeof(LodgingBooking).GetProperty(nameof(LodgingBooking.StayPeriod))?.SetValue(booking, stayPeriod);
        typeof(LodgingBooking).GetProperty(nameof(LodgingBooking.Cost))?.SetValue(booking, cost);
        typeof(LodgingBooking).GetProperty(nameof(LodgingBooking.Status))?.SetValue(booking, status);
        typeof(LodgingBooking).GetProperty(nameof(LodgingBooking.BookedAt))?.SetValue(booking, bookedAt);

        return booking;
    }

    private LodgingBookingDbo MapToDbo(LodgingBooking booking)
    {
        return new LodgingBookingDbo
        {
            Id = booking.Id.Value,
            PublicId = booking.PublicId?.ToString(),
            HotelName = booking.HotelName.Value,
            AddressStreet = booking.Address.Street,
            AddressCity = booking.Address.City,
            AddressState = booking.Address.State,
            AddressZipCode = booking.Address.ZipCode,
            StayPeriodStartUtc = booking.StayPeriod.Start.ToUnixTimeMilliseconds(),
            StayPeriodEndUtc = booking.StayPeriod.End.ToUnixTimeMilliseconds(),
            CostAmount = booking.Cost.Amount.ToString(),
            CostCurrency = booking.Cost.Currency,
            Status = booking.Status.Value,
            BookedAt = booking.BookedAt.ToUnixTimeMilliseconds()
        };
    }
}
