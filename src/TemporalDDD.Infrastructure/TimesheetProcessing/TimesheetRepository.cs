using Microsoft.EntityFrameworkCore;
using TemporalDDD.Domain.TimesheetProcessing;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.TimesheetProcessing;

public class TimesheetRepository : ITimesheetRepository
{
    private readonly ApplicationDbContext _dbContext;

    public TimesheetRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Timesheet?> GetByIdAsync(TimesheetId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Timesheets
            .FirstOrDefaultAsync(t => t.Id == id.Value, cancellationToken);
    }

    public async Task SaveAsync(Timesheet aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Timesheets
            .FirstOrDefaultAsync(t => t.Id == aggregate.Id.Value, cancellationToken);

        if (existing == null)
        {
            _dbContext.Timesheets.Add(aggregate);
        }
        else
        {
            _dbContext.Entry(existing).State = EntityState.Detached;
            _dbContext.Timesheets.Attach(aggregate);
            _dbContext.Entry(aggregate).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
