using Temporalio.Workflows;
using TemporalDDD.Application.PlacementMatching;
using TemporalDDD.Domain.PlacementMatching;
using TemporalDDD.Domain.PlacementMatching.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Application.PlacementMatching;

[Workflow]
public class PlacementMatchingWorkflow
{
    private AssignmentId? _assignmentId;
    private OfferAcceptedSignal? _offerAcceptedSignal;
    private OfferRejectedSignal? _offerRejectedSignal;
    private ProviderMatchedElsewhereSignal? _providerMatchedElsewhereSignal;

    [WorkflowRun]
    public async Task RunAsync(PlacementMatchingInput input)
    {
        // Elevate to Domain types with catastrophic assertions
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
        // Step 1: Calculate Match Score
        var matchScore = await Workflow.ExecuteActivityAsync(
            (IPlacementMatchingActivities activities) => activities.CalculateMatchScoreAsync(providerId, facilityId, positionId),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
        );

        // Step 2: Propose Assignment
        _assignmentId = await Workflow.ExecuteActivityAsync(
            (IPlacementMatchingActivities activities) => activities.ProposeAssignmentAsync(providerId, facilityId, positionId, matchScore),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
        );

        // Step 3: Wait for one of three signals
        await Workflow.WaitConditionAsync(() => 
            _offerAcceptedSignal is not null ||
            _offerRejectedSignal is not null ||
            _providerMatchedElsewhereSignal is not null
        );

        // Handle the received signal
        if (_offerAcceptedSignal is not null)
        {
            // Step 4a: Commit Assignment with OCC (Optimistic Concurrency Control)
            await Workflow.ExecuteActivityAsync(
                (IPlacementMatchingActivities activities) => activities.CommitAssignmentAsync(_assignmentId, AggregateVersion.Create(1).Value!),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );
        }
        else if (_offerRejectedSignal is not null || _providerMatchedElsewhereSignal is not null)
        {
            // Step 4b: Revoke Offer
            await Workflow.ExecuteActivityAsync(
                (IPlacementMatchingActivities activities) => activities.RevokeOfferAsync(_assignmentId),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );
        }
    }

    [WorkflowSignal]
    public async Task OfferAcceptedAsync()
    {
        _offerAcceptedSignal = new OfferAcceptedSignal();
    }

    [WorkflowSignal]
    public async Task OfferRejectedAsync()
    {
        _offerRejectedSignal = new OfferRejectedSignal();
    }

    [WorkflowSignal]
    public async Task ProviderMatchedElsewhereAsync()
    {
        _providerMatchedElsewhereSignal = new ProviderMatchedElsewhereSignal();
    }
}

public record OfferAcceptedSignal();
public record OfferRejectedSignal();
public record ProviderMatchedElsewhereSignal();
