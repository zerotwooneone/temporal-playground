using Microsoft.EntityFrameworkCore;
using TemporalDDD.Domain.TravelLogistics;
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
        return await _dbContext.LodgingBookings
            .FirstOrDefaultAsync(l => l.Id == id.Value, cancellationToken);
    }

    public async Task SaveAsync(LodgingBooking aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.LodgingBookings
            .FirstOrDefaultAsync(l => l.Id == aggregate.Id.Value, cancellationToken);

        if (existing == null)
        {
            _dbContext.LodgingBookings.Add(aggregate);
        }
        else
        {
            _dbContext.Entry(existing).State = EntityState.Detached;
            _dbContext.LodgingBookings.Attach(aggregate);
            _dbContext.Entry(aggregate).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
