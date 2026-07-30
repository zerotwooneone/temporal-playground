using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.PlacementMatching.ValueObjects;

namespace TemporalDDD.Domain.PlacementMatching;

public sealed class Assignment
{
    public AssignmentId Id { get; private set; }
    public AssignmentPublicId? PublicId { get; private set; }
    public ProviderId ProviderId { get; private set; }
    public FacilityId FacilityId { get; private set; }
    public PositionId PositionId { get; private set; }
    public MatchScore MatchScore { get; private set; }
    public AssignmentStatus Status { get; private set; }
    public DateTimeOffset ProposedAt { get; private set; }
    public DateTimeOffset? AcceptedAt { get; private set; }
    public AggregateVersion Version { get; private set; }

    internal Assignment() { }

    // Internal constructor for infrastructure rehydration
    internal Assignment(AssignmentId id, AssignmentPublicId? publicId, ProviderId providerId, FacilityId facilityId, PositionId positionId, MatchScore matchScore, AssignmentStatus status, DateTimeOffset proposedAt, DateTimeOffset? acceptedAt, AggregateVersion version)
    {
        Id = id;
        PublicId = publicId;
        ProviderId = providerId;
        FacilityId = facilityId;
        PositionId = positionId;
        MatchScore = matchScore;
        Status = status;
        ProposedAt = proposedAt;
        AcceptedAt = acceptedAt;
        Version = version;
    }

    // Factory for creating new assignment (ID is client-generated)
    public static Assignment Create(ProviderId providerId, FacilityId facilityId, PositionId positionId, MatchScore matchScore)
    {
        return new Assignment
        {
            Id = AssignmentId.New(),
            PublicId = AssignmentPublicId.New(),
            ProviderId = providerId,
            FacilityId = facilityId,
            PositionId = positionId,
            MatchScore = matchScore,
            Status = AssignmentStatus.Proposed,
            ProposedAt = DateTimeOffset.UtcNow,
            Version = AggregateVersion.Initial()
        };
    }

    public void Accept(AggregateVersion expectedVersion)
    {
        if (Status != AssignmentStatus.Proposed)
            throw new InvalidOperationException($"Cannot accept assignment in status: {Status}");

        if (Version != expectedVersion)
            throw new InvalidOperationException($"Version mismatch. Expected {expectedVersion}, actual {Version}");

        Status = AssignmentStatus.Accepted;
        AcceptedAt = DateTimeOffset.UtcNow;
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
