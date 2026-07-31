using Microsoft.EntityFrameworkCore;
using TemporalDDD.Domain.TravelLogistics;
using TemporalDDD.Domain.TravelLogistics.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.TravelLogistics;

public class FlightBookingRepository : IFlightBookingRepository
{
    private readonly ApplicationDbContext _dbContext;

    public FlightBookingRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FlightBooking?> GetByIdAsync(FlightBookingId id, CancellationToken cancellationToken = default)
    {
        var dbo = await _dbContext.FlightBookings
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id.ToString(), cancellationToken);
        
        if (dbo == null) return null;
        
        return MapToDomain(dbo);
    }

    public async Task SaveAsync(FlightBooking aggregate, CancellationToken cancellationToken = default)
    {
        var dbo = MapToDbo(aggregate);
        var idString = aggregate.Id.ToString();
        var existing = await _dbContext.FlightBookings
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == idString, cancellationToken);

        if (existing == null)
        {
            _dbContext.FlightBookings.Add(dbo);
        }
        else
        {
            _dbContext.FlightBookings.Update(dbo);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private FlightBooking MapToDomain(FlightBookingDbo dbo)
    {
        var flightNumber = FlightNumber.Create(dbo.FlightNumber).Value ?? throw new InvalidOperationException($"Invalid flight number in database: {dbo.FlightNumber}");
        var origin = AirportCode.Create(dbo.Origin).Value ?? throw new InvalidOperationException($"Invalid origin airport code in database: {dbo.Origin}");
        var destination = AirportCode.Create(dbo.Destination).Value ?? throw new InvalidOperationException($"Invalid destination airport code in database: {dbo.Destination}");
        var departureTime = FlightDepartureTime.Create(DateTimeOffset.FromUnixTimeMilliseconds(dbo.DepartureTime)).Value ?? throw new InvalidOperationException($"Invalid departure time in database: {dbo.DepartureTime}");
        var cost = Money.Create(decimal.Parse(dbo.CostAmount), dbo.CostCurrency).Value ?? throw new InvalidOperationException($"Invalid cost in database: {dbo.CostAmount} {dbo.CostCurrency}");
        var status = BookingStatus.FromValue(dbo.Status);
        var bookedAt = DateTimeOffset.FromUnixTimeMilliseconds(dbo.BookedAt);

        FlightBookingPublicId? publicId = null;
        if (!string.IsNullOrEmpty(dbo.PublicId))
        {
            publicId = FlightBookingPublicId.FromString(dbo.PublicId);
        }

        var id = FlightBookingId.Create(dbo.Id).Value ?? throw new InvalidOperationException($"Invalid flight booking ID in database: {dbo.Id}");

        // Use internal constructor for rehydration (infrastructure concern)
        return new FlightBooking(
            id: id,
            publicId: publicId,
            flightNumber: flightNumber,
            origin: origin,
            destination: destination,
            departureTime: departureTime,
            cost: cost,
            status: status,
            bookedAt: bookedAt
        );
    }

    private FlightBookingDbo MapToDbo(FlightBooking booking)
    {
        return new FlightBookingDbo
        {
            Id = booking.Id.ToString(),
            PublicId = booking.PublicId?.ToString(),
            FlightNumber = booking.FlightNumber.Value,
            Origin = booking.Origin.Value,
            Destination = booking.Destination.Value,
            DepartureTime = booking.DepartureTime.Value.ToUnixTimeMilliseconds(),
            CostAmount = booking.Cost.Amount.ToString(),
            CostCurrency = booking.Cost.Currency,
            Status = booking.Status.Value,
            BookedAt = booking.BookedAt.ToUnixTimeMilliseconds()
        };
    }
}
