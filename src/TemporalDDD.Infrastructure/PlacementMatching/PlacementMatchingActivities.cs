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

    [Activity]
    public async Task<decimal> CalculateMatchScoreAsync(CalculateMatchScoreInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var providerIdResult = ProviderId.Create(input.ProviderId);
        if (providerIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid ProviderId. {providerIdResult.Error}");
        
        var facilityIdResult = FacilityId.Create(input.FacilityId);
        if (facilityIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid FacilityId. {facilityIdResult.Error}");
        
        var positionIdResult = PositionId.Create(input.PositionId);
        if (positionIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid PositionId. {positionIdResult.Error}");

        var providerId = providerIdResult.Value;
        var facilityId = facilityIdResult.Value;
        var positionId = positionIdResult.Value;

        // Simulate match score calculation algorithm
        await Task.Delay(500);

        // In real implementation, this would use ML model or business rules
        var random = new Random();
        var matchScoreValue = (decimal)(random.Next(60, 100) + (random.NextDouble() * 0.9));
        Console.WriteLine($"[MatchScore] Calculated score {matchScoreValue:F2} for provider {providerId.Value} at facility {facilityId.Value}");

        return matchScoreValue;
    }

    [Activity]
    public async Task<string> ProposeAssignmentAsync(ProposeAssignmentInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var providerIdResult = ProviderId.Create(input.ProviderId);
        if (providerIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid ProviderId. {providerIdResult.Error}");
        
        var facilityIdResult = FacilityId.Create(input.FacilityId);
        if (facilityIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid FacilityId. {facilityIdResult.Error}");
        
        var positionIdResult = PositionId.Create(input.PositionId);
        if (positionIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid PositionId. {positionIdResult.Error}");
        
        var providerId = providerIdResult.Value;
        var facilityId = facilityIdResult.Value;
        var positionId = positionIdResult.Value;
        var matchScoreResult = MatchScore.Create(input.MatchScore);
        if (matchScoreResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid MatchScore. {matchScoreResult.Error}");
        var matchScore = matchScoreResult.Value;

        // Create domain entity using factory
        var assignment = Assignment.Create(providerId, facilityId, positionId, matchScore);

        await _assignmentRepository.SaveAsync(assignment);

        Console.WriteLine($"[Assignment] Proposed assignment {assignment.Id} for provider {providerId.Value} at facility {facilityId.Value} (Score: {input.MatchScore:F2})");

        return assignment.Id.ToString();
    }

    [Activity]
    public async Task CommitAssignmentAsync(CommitAssignmentInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var assignmentIdResult = AssignmentId.Create(input.AssignmentId);
        if (assignmentIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid AssignmentId. {assignmentIdResult.Error}");
        
        var expectedVersionResult = AggregateVersion.Create(input.ExpectedVersion);
        if (expectedVersionResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid AggregateVersion. {expectedVersionResult.Error}");

        var assignmentId = assignmentIdResult.Value;
        var expectedVersion = expectedVersionResult.Value;

        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);

        if (assignment == null)
        {
            throw new InvalidOperationException($"Assignment {assignmentId} not found");
        }

        // Check version for OCC and accept the assignment
        assignment.Accept(expectedVersion);

        await _assignmentRepository.SaveAsync(assignment);

        Console.WriteLine($"[Assignment] Committed assignment {assignmentId} with version check {expectedVersion.Value}");
    }

    [Activity]
    public async Task RevokeOfferAsync(RevokeOfferInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var assignmentIdResult = AssignmentId.Create(input.AssignmentId);
        if (assignmentIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid AssignmentId. {assignmentIdResult.Error}");

        var assignmentId = assignmentIdResult.Value;

        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId);

        if (assignment == null)
        {
            throw new InvalidOperationException($"Assignment {assignmentId} not found");
        }

        assignment.Revoke();

        await _assignmentRepository.SaveAsync(assignment);

        Console.WriteLine($"[Assignment] Revoked offer {assignmentId}");
    }
}
