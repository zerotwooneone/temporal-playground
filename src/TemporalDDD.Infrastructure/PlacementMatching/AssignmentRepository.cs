using Microsoft.EntityFrameworkCore;
using TemporalDDD.Domain.PlacementMatching;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.PlacementMatching;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AssignmentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Assignment?> GetByIdAsync(AssignmentId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Assignments
            .FirstOrDefaultAsync(a => a.Id == id.Value, cancellationToken);
    }

    public async Task SaveAsync(Assignment aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Assignments
            .FirstOrDefaultAsync(a => a.Id == aggregate.Id.Value, cancellationToken);

        if (existing == null)
        {
            _dbContext.Assignments.Add(aggregate);
        }
        else
        {
            _dbContext.Entry(existing).State = EntityState.Detached;
            _dbContext.Assignments.Attach(aggregate);
            _dbContext.Entry(aggregate).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
