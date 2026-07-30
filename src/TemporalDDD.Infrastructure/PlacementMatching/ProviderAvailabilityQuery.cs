using Microsoft.EntityFrameworkCore;
using TemporalDDD.Application.PlacementMatching;
using TemporalDDD.Domain.PlacementMatching;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.PlacementMatching;

public class ProviderAvailabilityQuery : IProviderAvailabilityQuery
{
    private readonly ApplicationDbContext _dbContext;

    public ProviderAvailabilityQuery(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProviderAvailabilityDto> GetProviderAvailabilityAsync(ProviderId providerId, DateRange period, CancellationToken cancellationToken = default)
    {
        // Check for conflicting assignments in the database
        var conflictingAssignment = await _dbContext.Assignments
            .Where(a => a.ProviderId == providerId.Value &&
                       a.Status == AssignmentStatus.Accepted &&
                       a.ProposedAt >= period.Start &&
                       a.ProposedAt <= period.End)
            .FirstOrDefaultAsync(cancellationToken);

        return new ProviderAvailabilityDto(
            IsAvailable: conflictingAssignment == null,
            ConflictingAssignmentId: conflictingAssignment?.PublicId
        );
    }
}
