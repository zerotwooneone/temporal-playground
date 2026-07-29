using TemporalDDD.Domain.PlacementMatching.ValueObjects;

namespace TemporalDDD.Domain.PlacementMatching;

public class Assignment
{
    public Guid Id { get; private set; }
    public Guid ProviderId { get; private set; }
    public Guid FacilityId { get; private set; }
    public Guid PositionId { get; private set; }
    public MatchScore MatchScore { get; private set; }
    public AssignmentStatus Status { get; private set; }
    public DateTime ProposedAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public AggregateVersion Version { get; private set; }

    private Assignment() { }

    public Assignment(Guid providerId, Guid facilityId, Guid positionId, MatchScore matchScore)
    {
        Id = Guid.NewGuid();
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

public enum AssignmentStatus
{
    Proposed,
    Accepted,
    Rejected,
    Revoked
}
