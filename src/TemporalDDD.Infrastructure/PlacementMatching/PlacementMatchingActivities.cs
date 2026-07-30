using Microsoft.EntityFrameworkCore;
using Temporalio.Activities;
using TemporalDDD.Application.PlacementMatching;
using TemporalDDD.Domain.PlacementMatching;
using TemporalDDD.Domain.PlacementMatching.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Infrastructure.PlacementMatching;

public class PlacementMatchingActivities : IPlacementMatchingActivities
{
    private readonly IAssignmentRepository _assignmentRepository;

    public PlacementMatchingActivities(IAssignmentRepository assignmentRepository)
    {
        _assignmentRepository = assignmentRepository;
    }

    public async Task<MatchScore> CalculateMatchScoreAsync(ProviderId providerId, FacilityId facilityId, PositionId positionId)
    {
        // Simulate match score calculation algorithm
        await Task.Delay(500);

        // In real implementation, this would use ML model or business rules
        var random = new Random();
        var matchScoreValue = (decimal)(random.Next(60, 100) + (random.NextDouble() * 0.9));
        var matchScore = MatchScore.Create(matchScoreValue);

        Console.WriteLine($"[MatchScore] Calculated score {matchScore.Value:F2} for provider {providerId.Value} at facility {facilityId.Value}");

        return matchScore;
    }

    public async Task<AssignmentId> ProposeAssignmentAsync(ProviderId providerId, FacilityId facilityId, PositionId positionId, MatchScore matchScore)
    {
        // Create domain entity using factory
        var assignment = Assignment.Create(providerId, facilityId, positionId, matchScore);

        await _assignmentRepository.SaveAsync(assignment);

        Console.WriteLine($"[Assignment] Proposed assignment {assignment.Id.Value} for provider {providerId.Value} at facility {facilityId.Value} (Score: {matchScore.Value:F2})");

        return assignment.Id;
    }

    public async Task CommitAssignmentAsync(AssignmentId assignmentId, AggregateVersion expectedVersion)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);

        if (assignment == null)
        {
            throw new InvalidOperationException($"Assignment {assignmentId.Value} not found");
        }

        // Check version for OCC and accept the assignment
        assignment.Accept(expectedVersion);

        await _assignmentRepository.SaveAsync(assignment);

        Console.WriteLine($"[Assignment] Committed assignment {assignmentId.Value} with version check {expectedVersion.Value}");
    }

    public async Task RevokeOfferAsync(AssignmentId assignmentId)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);

        if (assignment == null)
        {
            throw new InvalidOperationException($"Assignment {assignmentId.Value} not found");
        }

        assignment.Revoke();

        await _assignmentRepository.SaveAsync(assignment);

        Console.WriteLine($"[Assignment] Revoked offer {assignmentId.Value}");
    }
}
