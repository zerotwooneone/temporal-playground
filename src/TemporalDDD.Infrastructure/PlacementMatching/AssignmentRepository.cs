using Microsoft.EntityFrameworkCore;
using TemporalDDD.Domain.PlacementMatching;
using TemporalDDD.Domain.PlacementMatching.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
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
        var dbo = await _dbContext.Assignments
            .FirstOrDefaultAsync(a => a.Id == id.Value, cancellationToken);
        
        if (dbo == null) return null;
        
        return MapToDomain(dbo);
    }

    public async Task SaveAsync(Assignment aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Assignments
            .FirstOrDefaultAsync(a => a.Id == aggregate.Id.Value, cancellationToken);

        var dbo = MapToDbo(aggregate);

        if (existing == null)
        {
            _dbContext.Assignments.Add(dbo);
        }
        else
        {
            _dbContext.Entry(existing).State = EntityState.Detached;
            _dbContext.Assignments.Attach(dbo);
            _dbContext.Entry(dbo).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private Assignment MapToDomain(AssignmentDbo dbo)
    {
        var providerId = ProviderId.Create(dbo.ProviderId).Value;
        var facilityId = FacilityId.Create(dbo.FacilityId).Value;
        var positionId = PositionId.Create(dbo.PositionId).Value;
        var matchScore = MatchScore.Create(dbo.MatchScore).Value;
        var status = AssignmentStatus.FromValue(dbo.Status);
        var proposedAt = DateTimeOffset.FromUnixTimeMilliseconds(dbo.ProposedAt);
        var acceptedAt = dbo.AcceptedAt.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(dbo.AcceptedAt.Value) : (DateTimeOffset?)null;
        var version = AggregateVersion.Create(dbo.Version);

        AssignmentPublicId? publicId = null;
        if (!string.IsNullOrEmpty(dbo.PublicId))
        {
            publicId = AssignmentPublicId.FromString(dbo.PublicId);
        }

        // Use reflection to call private constructor for rehydration
        var assignment = (Assignment)Activator.CreateInstance(
            typeof(Assignment),
            nonPublic: true)!;
        
        // Set properties via reflection (infrastructure concern)
        typeof(Assignment).GetProperty(nameof(Assignment.Id))?.SetValue(assignment, AssignmentId.Create(dbo.Id).Value);
        typeof(Assignment).GetProperty(nameof(Assignment.PublicId))?.SetValue(assignment, publicId);
        typeof(Assignment).GetProperty(nameof(Assignment.ProviderId))?.SetValue(assignment, providerId);
        typeof(Assignment).GetProperty(nameof(Assignment.FacilityId))?.SetValue(assignment, facilityId);
        typeof(Assignment).GetProperty(nameof(Assignment.PositionId))?.SetValue(assignment, positionId);
        typeof(Assignment).GetProperty(nameof(Assignment.MatchScore))?.SetValue(assignment, matchScore);
        typeof(Assignment).GetProperty(nameof(Assignment.Status))?.SetValue(assignment, status);
        typeof(Assignment).GetProperty(nameof(Assignment.ProposedAt))?.SetValue(assignment, proposedAt);
        typeof(Assignment).GetProperty(nameof(Assignment.AcceptedAt))?.SetValue(assignment, acceptedAt);
        typeof(Assignment).GetProperty(nameof(Assignment.Version))?.SetValue(assignment, version);

        return assignment;
    }

    private AssignmentDbo MapToDbo(Assignment assignment)
    {
        return new AssignmentDbo
        {
            Id = assignment.Id.Value,
            PublicId = assignment.PublicId?.ToString(),
            ProviderId = assignment.ProviderId.Value,
            FacilityId = assignment.FacilityId.Value,
            PositionId = assignment.PositionId.Value,
            MatchScore = assignment.MatchScore.Value,
            Status = assignment.Status.Value,
            ProposedAt = assignment.ProposedAt.ToUnixTimeMilliseconds(),
            AcceptedAt = assignment.AcceptedAt?.ToUnixTimeMilliseconds(),
            Version = assignment.Version.Value
        };
    }
}
