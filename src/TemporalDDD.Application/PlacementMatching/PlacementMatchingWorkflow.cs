using Temporalio.Workflows;

namespace TemporalDDD.Application.PlacementMatching;

[Workflow]
public class PlacementMatchingWorkflow
{
    private uint? _assignmentId;
    private OfferAcceptedSignal? _offerAcceptedSignal;
    private OfferRejectedSignal? _offerRejectedSignal;
    private ProviderMatchedElsewhereSignal? _providerMatchedElsewhereSignal;

    [WorkflowRun]
    public async Task RunAsync(PlacementMatchingInput input)
    {
        // Pass-through: No domain conversion needed - just pass primitives to activities
        // Step 1: Calculate Match Score
        var matchScore = await Workflow.ExecuteActivityAsync(
            (IPlacementMatchingActivities activities) => activities.CalculateMatchScoreAsync(new CalculateMatchScoreInput(input.ProviderId, input.FacilityId, input.PositionId)),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
        );

        // Step 2: Propose Assignment
        _assignmentId = await Workflow.ExecuteActivityAsync(
            (IPlacementMatchingActivities activities) => activities.ProposeAssignmentAsync(new ProposeAssignmentInput(input.ProviderId, input.FacilityId, input.PositionId, matchScore)),
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
                (IPlacementMatchingActivities activities) => activities.CommitAssignmentAsync(new CommitAssignmentInput(_assignmentId.Value, 1)),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );
        }
        else if (_offerRejectedSignal is not null || _providerMatchedElsewhereSignal is not null)
        {
            // Step 4b: Revoke Offer
            await Workflow.ExecuteActivityAsync(
                (IPlacementMatchingActivities activities) => activities.RevokeOfferAsync(new RevokeOfferInput(_assignmentId.Value)),
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
