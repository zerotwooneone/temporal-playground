using Temporalio.Activities;
using TemporalDDD.Domain.PlacementMatching;
using TemporalDDD.Domain.PlacementMatching.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Application.PlacementMatching;

public interface IPlacementMatchingActivities
{
    [Activity]
    Task<MatchScore> CalculateMatchScoreAsync(CalculateMatchScoreInput input);
    [Activity]
    Task<AssignmentId> ProposeAssignmentAsync(ProposeAssignmentInput input);
    [Activity]
    Task CommitAssignmentAsync(CommitAssignmentInput input);
    [Activity]
    Task RevokeOfferAsync(RevokeOfferInput input);
}

// Primitive DTOs for activity parameters
public record CalculateMatchScoreInput(uint ProviderId, uint FacilityId, uint PositionId);
public record ProposeAssignmentInput(uint ProviderId, uint FacilityId, uint PositionId, decimal MatchScore);
public record CommitAssignmentInput(uint AssignmentId, int ExpectedVersion);
public record RevokeOfferInput(uint AssignmentId);
