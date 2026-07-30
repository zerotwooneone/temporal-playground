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
            .FirstOrDefaultAsync(f => f.Id == id.Value, cancellationToken);
        
        if (dbo == null) return null;
        
        return MapToDomain(dbo);
    }

    public async Task SaveAsync(FlightBooking aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.FlightBookings
            .FirstOrDefaultAsync(f => f.Id == aggregate.Id.Value, cancellationToken);

        var dbo = MapToDbo(aggregate);

        if (existing == null)
        {
            _dbContext.FlightBookings.Add(dbo);
        }
        else
        {
            _dbContext.Entry(existing).State = EntityState.Detached;
            _dbContext.FlightBookings.Attach(dbo);
            _dbContext.Entry(dbo).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private FlightBooking MapToDomain(FlightBookingDbo dbo)
    {
        var flightNumber = FlightNumber.Create(dbo.FlightNumber).Value;
        var origin = AirportCode.Create(dbo.Origin).Value;
        var destination = AirportCode.Create(dbo.Destination).Value;
        var departureTime = FlightDepartureTime.Create(DateTimeOffset.FromUnixTimeMilliseconds(dbo.DepartureTime)).Value;
        var cost = Money.Create(decimal.Parse(dbo.CostAmount), dbo.CostCurrency);
        var status = BookingStatus.FromValue(dbo.Status);
        var bookedAt = DateTimeOffset.FromUnixTimeMilliseconds(dbo.BookedAt);

        FlightBookingPublicId? publicId = null;
        if (!string.IsNullOrEmpty(dbo.PublicId))
        {
            publicId = FlightBookingPublicId.FromString(dbo.PublicId);
        }

        // Use reflection to call private constructor for rehydration
        var booking = (FlightBooking)Activator.CreateInstance(
            typeof(FlightBooking),
            nonPublic: true)!;
        
        // Set properties via reflection (infrastructure concern)
        typeof(FlightBooking).GetProperty(nameof(FlightBooking.Id))?.SetValue(booking, FlightBookingId.Create(dbo.Id).Value);
        typeof(FlightBooking).GetProperty(nameof(FlightBooking.PublicId))?.SetValue(booking, publicId);
        typeof(FlightBooking).GetProperty(nameof(FlightBooking.FlightNumber))?.SetValue(booking, flightNumber);
        typeof(FlightBooking).GetProperty(nameof(FlightBooking.Origin))?.SetValue(booking, origin);
        typeof(FlightBooking).GetProperty(nameof(FlightBooking.Destination))?.SetValue(booking, destination);
        typeof(FlightBooking).GetProperty(nameof(FlightBooking.DepartureTime))?.SetValue(booking, departureTime);
        typeof(FlightBooking).GetProperty(nameof(FlightBooking.Cost))?.SetValue(booking, cost);
        typeof(FlightBooking).GetProperty(nameof(FlightBooking.Status))?.SetValue(booking, status);
        typeof(FlightBooking).GetProperty(nameof(FlightBooking.BookedAt))?.SetValue(booking, bookedAt);

        return booking;
    }

    private FlightBookingDbo MapToDbo(FlightBooking booking)
    {
        return new FlightBookingDbo
        {
            Id = booking.Id.Value,
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
