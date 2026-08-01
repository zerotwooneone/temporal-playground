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
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id.ToString(), cancellationToken);
        
        if (dbo == null) return null;
        
        return MapToDomain(dbo);
    }

    public async Task SaveAsync(LodgingBooking aggregate, CancellationToken cancellationToken = default)
    {
        var idString = aggregate.Id.ToString();
        var existing = await _dbContext.LodgingBookings
            .FirstOrDefaultAsync(l => l.Id == idString, cancellationToken);

        if (existing == null)
        {
            existing = new LodgingBookingDbo();
            MapToDbo(aggregate, existing);
            _dbContext.LodgingBookings.Add(existing);
        }
        else
        {
            MapToDbo(aggregate, existing);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private LodgingBooking MapToDomain(LodgingBookingDbo dbo)
    {
        var hotelName = HotelName.Create(dbo.HotelName).Value ?? throw new InvalidOperationException($"Invalid hotel name in database: {dbo.HotelName}");
        var address = Address.Create(dbo.AddressStreet, dbo.AddressCity, dbo.AddressState, dbo.AddressZipCode).Value ?? throw new InvalidOperationException($"Invalid address in database: {dbo.AddressStreet}, {dbo.AddressCity}, {dbo.AddressState}, {dbo.AddressZipCode}");
        var stayPeriodStart = DateTimeOffset.FromUnixTimeMilliseconds(dbo.StayPeriodStartUtc);
        var stayPeriodEnd = DateTimeOffset.FromUnixTimeMilliseconds(dbo.StayPeriodEndUtc);
        var stayPeriod = DateRange.Create(stayPeriodStart, stayPeriodEnd).Value ?? throw new InvalidOperationException($"Invalid stay period in database: {stayPeriodStart} to {stayPeriodEnd}");
        var cost = Money.Create(decimal.Parse(dbo.CostAmount), dbo.CostCurrency).Value ?? throw new InvalidOperationException($"Invalid cost in database: {dbo.CostAmount} {dbo.CostCurrency}");
        var status = BookingStatus.FromValue(dbo.Status);
        var bookedAt = DateTimeOffset.FromUnixTimeMilliseconds(dbo.BookedAt);

        LodgingBookingPublicId? publicId = null;
        if (!string.IsNullOrEmpty(dbo.PublicId))
        {
            publicId = LodgingBookingPublicId.FromString(dbo.PublicId);
        }

        var id = LodgingBookingId.Create(dbo.Id).Value ?? throw new InvalidOperationException($"Invalid lodging booking ID in database: {dbo.Id}");

        // Use internal constructor for rehydration (infrastructure concern)
        return new LodgingBooking(
            id: id,
            publicId: publicId,
            hotelName: hotelName,
            address: address,
            stayPeriod: stayPeriod,
            cost: cost,
            status: status,
            bookedAt: bookedAt
        );
    }

    private void MapToDbo(LodgingBooking booking, LodgingBookingDbo dbo)
    {
        dbo.Id = booking.Id.ToString();
        dbo.PublicId = booking.PublicId?.ToString();
        dbo.HotelName = booking.HotelName.Value;
        dbo.AddressStreet = booking.Address.Street;
        dbo.AddressCity = booking.Address.City;
        dbo.AddressState = booking.Address.State;
        dbo.AddressZipCode = booking.Address.ZipCode;
        dbo.StayPeriodStartUtc = booking.StayPeriod.Start.ToUnixTimeMilliseconds();
        dbo.StayPeriodEndUtc = booking.StayPeriod.End.ToUnixTimeMilliseconds();
        dbo.CostAmount = booking.Cost.Amount.ToString();
        dbo.CostCurrency = booking.Cost.Currency;
        dbo.Status = booking.Status.Value;
        dbo.BookedAt = booking.BookedAt.ToUnixTimeMilliseconds();
    }
}
