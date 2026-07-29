using Temporalio.Activities;

namespace TemporalDDD.Application.PlacementMatching;

public interface IPlacementMatchingActivities
{
    Task<decimal> CalculateMatchScoreAsync(uint providerId, uint facilityId, uint positionId);
    Task<uint> ProposeAssignmentAsync(uint providerId, uint facilityId, uint positionId, decimal matchScore);
    Task CommitAssignmentAsync(uint assignmentId, int expectedVersion);
    Task RevokeOfferAsync(uint assignmentId);
}
