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

    public async Task<ProviderAvailabilityDto> GetProviderAvailabilityAsync(ProviderId providerId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default)
    {
        // Convert DateTimeOffset to Unix milliseconds for comparison with DBO
        var startMs = start.ToUnixTimeMilliseconds();
        var endMs = end.ToUnixTimeMilliseconds();

        // Check for conflicting assignments in the database
        var conflictingAssignment = await _dbContext.Assignments
            .Where(a => a.ProviderId == providerId.Value &&
                       a.Status == AssignmentStatus.Accepted &&
                       a.ProposedAt >= startMs &&
                       a.ProposedAt <= endMs)
            .FirstOrDefaultAsync(cancellationToken);

        AssignmentPublicId? conflictingPublicId = null;
        if (!string.IsNullOrEmpty(conflictingAssignment?.PublicId))
        {
            conflictingPublicId = AssignmentPublicId.FromString(conflictingAssignment.PublicId);
        }

        return new ProviderAvailabilityDto(
            IsAvailable: conflictingAssignment == null,
            ConflictingAssignmentId: conflictingPublicId
        );
    }
}
