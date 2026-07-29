using Temporalio.Workflows;
using TemporalDDD.Application.PlacementMatching;

namespace TemporalDDD.Application.PlacementMatching;

[Workflow]
public class PlacementMatchingWorkflow
{
    private Guid? _assignmentId;
    private OfferAcceptedSignal? _offerAcceptedSignal;
    private OfferRejectedSignal? _offerRejectedSignal;
    private ProviderMatchedElsewhereSignal? _providerMatchedElsewhereSignal;

    [WorkflowRun]
    public async Task RunAsync(Guid providerId, Guid facilityId, Guid positionId)
    {
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
                (IPlacementMatchingActivities activities) => activities.CommitAssignmentAsync(_assignmentId.Value, expectedVersion: 1),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );
        }
        else if (_offerRejectedSignal is not null || _providerMatchedElsewhereSignal is not null)
        {
            // Step 4b: Revoke Offer
            await Workflow.ExecuteActivityAsync(
                (IPlacementMatchingActivities activities) => activities.RevokeOfferAsync(_assignmentId.Value),
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
