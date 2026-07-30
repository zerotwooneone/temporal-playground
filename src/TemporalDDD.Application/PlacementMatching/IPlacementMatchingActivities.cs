using Temporalio.Activities;
using TemporalDDD.Domain.PlacementMatching;
using TemporalDDD.Domain.PlacementMatching.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Application.PlacementMatching;

public interface IPlacementMatchingActivities
{
    Task<MatchScore> CalculateMatchScoreAsync(ProviderId providerId, FacilityId facilityId, PositionId positionId);
    Task<AssignmentId> ProposeAssignmentAsync(ProviderId providerId, FacilityId facilityId, PositionId positionId, MatchScore matchScore);
    Task CommitAssignmentAsync(AssignmentId assignmentId, AggregateVersion expectedVersion);
    Task RevokeOfferAsync(AssignmentId assignmentId);
}
