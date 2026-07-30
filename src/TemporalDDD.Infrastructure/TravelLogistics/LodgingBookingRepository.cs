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
            .FirstOrDefaultAsync(l => l.Id == id.Value.ToString(), cancellationToken);
        
        if (dbo == null) return null;
        
        return MapToDomain(dbo);
    }

    public async Task SaveAsync(LodgingBooking aggregate, CancellationToken cancellationToken = default)
    {
        var dbo = MapToDbo(aggregate);
        var id = aggregate.Id.Value;
        var existing = await _dbContext.LodgingBookings
            .FirstOrDefaultAsync(l => l.Id == id.ToString(), cancellationToken);

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

    private LodgingBookingDbo MapToDbo(LodgingBooking booking)
    {
        return new LodgingBookingDbo
        {
            Id = booking.Id.Value.ToString(),
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
