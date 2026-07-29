using Temporalio.Activities;

namespace TemporalDDD.Application.PlacementMatching;

public interface IPlacementMatchingActivities
{
    Task<decimal> CalculateMatchScoreAsync(Guid providerId, Guid facilityId, Guid positionId);
    Task<Guid> ProposeAssignmentAsync(Guid providerId, Guid facilityId, Guid positionId, decimal matchScore);
    Task CommitAssignmentAsync(Guid assignmentId, int expectedVersion);
    Task RevokeOfferAsync(Guid assignmentId);
}
