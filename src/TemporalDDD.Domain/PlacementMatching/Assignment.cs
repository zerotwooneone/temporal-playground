using TemporalDDD.Domain.PlacementMatching.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.PlacementMatching;

public class Assignment
{
    public AssignmentId Id { get; private set; }
    public ProviderId ProviderId { get; private set; }
    public FacilityId FacilityId { get; private set; }
    public PositionId PositionId { get; private set; }
    public MatchScore MatchScore { get; private set; }
    public AssignmentStatus Status { get; private set; }
    public DateTime ProposedAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public AggregateVersion Version { get; private set; }

    private Assignment() { }

    public Assignment(ProviderId providerId, FacilityId facilityId, PositionId positionId, MatchScore matchScore)
    {
        Id = AssignmentId.New();
        ProviderId = providerId;
        FacilityId = facilityId;
        PositionId = positionId;
        MatchScore = matchScore;
        Status = AssignmentStatus.Proposed;
        ProposedAt = DateTime.UtcNow;
        Version = AggregateVersion.Initial();
    }

    public void Accept(AggregateVersion expectedVersion)
    {
        if (Status != AssignmentStatus.Proposed)
            throw new InvalidOperationException($"Cannot accept assignment in status: {Status}");

        if (Version != expectedVersion)
            throw new InvalidOperationException($"Version mismatch. Expected {expectedVersion}, actual {Version}");

        Status = AssignmentStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;
        Version = Version.Increment();
    }

    public void Reject()
    {
        if (Status != AssignmentStatus.Proposed)
            throw new InvalidOperationException($"Cannot reject assignment in status: {Status}");

        Status = AssignmentStatus.Rejected;
        Version = Version.Increment();
    }

    public void Revoke()
    {
        if (Status != AssignmentStatus.Proposed)
            throw new InvalidOperationException($"Cannot revoke assignment in status: {Status}");

        Status = AssignmentStatus.Revoked;
        Version = Version.Increment();
    }
}
