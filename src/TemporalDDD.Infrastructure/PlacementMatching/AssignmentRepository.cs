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
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id.ToString(), cancellationToken);
        
        if (dbo == null) return null;
        
        return MapToDomain(dbo);
    }

    public async Task SaveAsync(Assignment aggregate, CancellationToken cancellationToken = default)
    {
        var dbo = MapToDbo(aggregate);
        var idString = aggregate.Id.ToString();
        var existing = await _dbContext.Assignments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == idString, cancellationToken);

        if (existing == null)
        {
            _dbContext.Assignments.Add(dbo);
        }
        else
        {
            var entry = _dbContext.Assignments.Update(dbo);
            // Tell EF Core what the original version was for optimistic concurrency control
            entry.OriginalValues[nameof(dbo.Version)] = existing.Version;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private Assignment MapToDomain(AssignmentDbo dbo)
    {
        var providerId = ProviderId.Create(dbo.ProviderId).Value ?? throw new InvalidOperationException($"Invalid provider ID in database: {dbo.ProviderId}");
        var facilityId = FacilityId.Create(dbo.FacilityId).Value ?? throw new InvalidOperationException($"Invalid facility ID in database: {dbo.FacilityId}");
        var positionId = PositionId.Create(dbo.PositionId).Value ?? throw new InvalidOperationException($"Invalid position ID in database: {dbo.PositionId}");
        var matchScore = MatchScore.Create(dbo.MatchScore).Value ?? throw new InvalidOperationException($"Invalid match score in database: {dbo.MatchScore}");
        var status = AssignmentStatus.FromValue(dbo.Status);
        var proposedAt = DateTimeOffset.FromUnixTimeMilliseconds(dbo.ProposedAt);
        var acceptedAt = dbo.AcceptedAt.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(dbo.AcceptedAt.Value) : (DateTimeOffset?)null;
        var version = AggregateVersion.Create(dbo.Version).Value ?? throw new InvalidOperationException($"Invalid version in database: {dbo.Version}");

        AssignmentPublicId? publicId = null;
        if (!string.IsNullOrEmpty(dbo.PublicId))
        {
            publicId = AssignmentPublicId.FromString(dbo.PublicId);
        }

        var id = AssignmentId.Create(dbo.Id).Value ?? throw new InvalidOperationException($"Invalid assignment ID in database: {dbo.Id}");

        // Use internal constructor for rehydration (infrastructure concern)
        return new Assignment(
            id: id,
            publicId: publicId,
            providerId: providerId,
            facilityId: facilityId,
            positionId: positionId,
            matchScore: matchScore,
            status: status,
            proposedAt: proposedAt,
            acceptedAt: acceptedAt,
            version: version
        );
    }

    private AssignmentDbo MapToDbo(Assignment assignment)
    {
        return new AssignmentDbo
        {
            Id = assignment.Id.ToString(),
            PublicId = assignment.PublicId?.ToString(),
            ProviderId = assignment.ProviderId.ToString(),
            FacilityId = assignment.FacilityId.ToString(),
            PositionId = assignment.PositionId.ToString(),
            MatchScore = assignment.MatchScore.Value,
            Status = assignment.Status.Value,
            ProposedAt = assignment.ProposedAt.ToUnixTimeMilliseconds(),
            AcceptedAt = assignment.AcceptedAt?.ToUnixTimeMilliseconds(),
            Version = assignment.Version.Value
        };
    }
}
