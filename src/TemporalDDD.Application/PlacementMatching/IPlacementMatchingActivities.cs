using Temporalio.Activities;

namespace TemporalDDD.Application.PlacementMatching;

public interface IPlacementMatchingActivities
{
    [Activity]
    Task<decimal> CalculateMatchScoreAsync(CalculateMatchScoreInput input);
    [Activity]
    Task<string> ProposeAssignmentAsync(ProposeAssignmentInput input);
    [Activity]
    Task CommitAssignmentAsync(CommitAssignmentInput input);
    [Activity]
    Task RevokeOfferAsync(RevokeOfferInput input);
}

// Primitive DTOs for activity parameters
public record CalculateMatchScoreInput(string ProviderId, string FacilityId, string PositionId);
public record ProposeAssignmentInput(string ProviderId, string FacilityId, string PositionId, decimal MatchScore);
public record CommitAssignmentInput(string AssignmentId, int ExpectedVersion);
public record RevokeOfferInput(string AssignmentId);
