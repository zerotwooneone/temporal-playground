using Temporalio.Activities;

namespace TemporalDDD.Application.PlacementMatching;

public interface IPlacementMatchingActivities
{
    [Activity]
    Task<decimal> CalculateMatchScoreAsync(CalculateMatchScoreInput input);
    [Activity]
    Task<uint> ProposeAssignmentAsync(ProposeAssignmentInput input);
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
