using Temporalio.Activities;
using TemporalDDD.Domain.PlacementMatching;
using TemporalDDD.Domain.PlacementMatching.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Application.PlacementMatching;

public interface IPlacementMatchingActivities
{
    [Activity]
    Task<MatchScore> CalculateMatchScoreAsync(ProviderId providerId, FacilityId facilityId, PositionId positionId);
    [Activity]
    Task<AssignmentId> ProposeAssignmentAsync(ProviderId providerId, FacilityId facilityId, PositionId positionId, MatchScore matchScore);
    [Activity]
    Task CommitAssignmentAsync(AssignmentId assignmentId, AggregateVersion expectedVersion);
    [Activity]
    Task RevokeOfferAsync(AssignmentId assignmentId);
}
