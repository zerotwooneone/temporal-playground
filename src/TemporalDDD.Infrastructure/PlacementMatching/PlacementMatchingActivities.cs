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

    public async Task<decimal> CalculateMatchScoreAsync(uint providerId, uint facilityId, uint positionId)
    {
        // Simulate match score calculation algorithm
        await Task.Delay(500);

        // In real implementation, this would use ML model or business rules
        var random = new Random();
        var matchScore = (decimal)(random.Next(60, 100) + (random.NextDouble() * 0.9));

        Console.WriteLine($"[MatchScore] Calculated score {matchScore:F2} for provider {providerId} at facility {facilityId}");

        return matchScore;
    }

    public async Task<uint> ProposeAssignmentAsync(uint providerId, uint facilityId, uint positionId, decimal matchScore)
    {
        // Create value objects
        var matchScoreVo = MatchScore.Create(matchScore);
        var providerIdVo = ProviderId.Create(providerId);
        var facilityIdVo = FacilityId.Create(facilityId);
        var positionIdVo = PositionId.Create(positionId);

        // Create domain entity using factory
        var assignment = Assignment.Create(providerIdVo, facilityIdVo, positionIdVo, matchScoreVo);

        await _assignmentRepository.SaveAsync(assignment);

        Console.WriteLine($"[Assignment] Proposed assignment {assignment.Id} for provider {providerId} at facility {facilityId} (Score: {matchScore:F2})");

        return assignment.Id;
    }

    public async Task CommitAssignmentAsync(uint assignmentId, int expectedVersion)
    {
        var assignmentIdVo = AssignmentId.Create(assignmentId);
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentIdVo);

        if (assignment == null)
        {
            throw new InvalidOperationException($"Assignment {assignmentId} not found");
        }

        // Check version for OCC and accept the assignment
        var expectedVersionVo = AggregateVersion.Create(expectedVersion);
        assignment.Accept(expectedVersionVo);

        await _assignmentRepository.SaveAsync(assignment);

        Console.WriteLine($"[Assignment] Committed assignment {assignmentId} with version check {expectedVersion}");
    }

    public async Task RevokeOfferAsync(uint assignmentId)
    {
        var assignmentIdVo = AssignmentId.Create(assignmentId);
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentIdVo);

        if (assignment == null)
        {
            throw new InvalidOperationException($"Assignment {assignmentId} not found");
        }

        assignment.Revoke();

        await _assignmentRepository.SaveAsync(assignment);

        Console.WriteLine($"[Assignment] Revoked offer {assignmentId}");
    }
}
