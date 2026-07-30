using Microsoft.EntityFrameworkCore;
using TemporalDDD.Domain.TravelLogistics;
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
        return await _dbContext.FlightBookings
            .FirstOrDefaultAsync(f => f.Id == id.Value, cancellationToken);
    }

    public async Task SaveAsync(FlightBooking aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.FlightBookings
            .FirstOrDefaultAsync(f => f.Id == aggregate.Id.Value, cancellationToken);

        if (existing == null)
        {
            _dbContext.FlightBookings.Add(aggregate);
        }
        else
        {
            _dbContext.Entry(existing).State = EntityState.Detached;
            _dbContext.FlightBookings.Attach(aggregate);
            _dbContext.Entry(aggregate).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
