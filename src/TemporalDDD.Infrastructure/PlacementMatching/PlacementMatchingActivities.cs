using Microsoft.EntityFrameworkCore;
using Temporalio.Activities;
using TemporalDDD.Application.PlacementMatching;
using TemporalDDD.Domain.PlacementMatching.ValueObjects;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.PlacementMatching;

public class PlacementMatchingActivities : IPlacementMatchingActivities
{
    private readonly ApplicationDbContext _dbContext;

    public PlacementMatchingActivities(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<decimal> CalculateMatchScoreAsync(Guid providerId, Guid facilityId, Guid positionId)
    {
        // Simulate match score calculation algorithm
        await Task.Delay(500);

        // In real implementation, this would use ML model or business rules
        var random = new Random();
        var matchScore = (decimal)(random.Next(60, 100) + (random.NextDouble() * 0.9));

        Console.WriteLine($"[MatchScore] Calculated score {matchScore:F2} for provider {providerId} at facility {facilityId}");

        return matchScore;
    }

    public async Task<Guid> ProposeAssignmentAsync(Guid providerId, Guid facilityId, Guid positionId, decimal matchScore)
    {
        // Simulate database operation to create assignment proposal
        await _dbContext.Database.EnsureCreatedAsync();

        // Create value object
        var matchScoreVo = MatchScore.Create(matchScore);

        // Create domain entity
        var assignment = new Domain.PlacementMatching.Assignment(providerId, facilityId, positionId, matchScoreVo);

        // Note: DbSet would be added to ApplicationDbContext once schema is defined
        // For now, we simulate the save
        await Task.Delay(100);

        Console.WriteLine($"[Assignment] Proposed assignment {assignment.Id} for provider {providerId} at facility {facilityId} (Score: {matchScore:F2})");

        return assignment.Id;
    }

    public async Task CommitAssignmentAsync(Guid assignmentId, int expectedVersion)
    {
        // Simulate database operation with Optimistic Concurrency Control (OCC)
        await _dbContext.Database.EnsureCreatedAsync();

        // Create value object
        var expectedVersionVo = AggregateVersion.Create(expectedVersion);

        // In real implementation, this would:
        // 1. Load the assignment from database
        // 2. Check if Version == expectedVersion
        // 3. If not, throw ConcurrencyException
        // 4. If yes, update Status to Accepted and increment Version
        // 5. Save changes

        await Task.Delay(100);

        // Simulate OCC check
        var random = new Random();
        if (random.Next(1, 100) == 1) // 1% chance of version conflict for testing
        {
            throw new DbUpdateConcurrencyException($"Optimistic concurrency violation: Assignment {assignmentId} was modified by another process");
        }

        Console.WriteLine($"[Assignment] Committed assignment {assignmentId} with version check {expectedVersion}");
    }

    public async Task RevokeOfferAsync(Guid assignmentId)
    {
        // Simulate database operation to revoke offer
        await _dbContext.Database.EnsureCreatedAsync();

        // In real implementation, this would load the assignment and call Revoke()
        await Task.Delay(100);

        Console.WriteLine($"[Assignment] Revoked offer {assignmentId}");
    }
}
