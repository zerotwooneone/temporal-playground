using Temporalio.Workflows;

namespace TemporalDDD.Application.WorkflowOrchestration;

[Workflow]
public class UpdateWorkflowNodesWorkflow
{
    [WorkflowRun]
    public async Task RunAsync(UpdateWorkflowNodesInput input)
    {
        var activityOptions = new ActivityOptions
        {
            StartToCloseTimeout = TimeSpan.FromMinutes(5)
        };

        // Execute UpdateNodesAndSaveAsync activity
        var saveResult = await Workflow.ExecuteActivityAsync(
            (IWorkflowOrchestrationActivities activities) => activities.UpdateNodesAndSaveAsync(input),
            activityOptions);

        // Execute PublishApplicationEventsAsync activity with the events from the first activity
        var publishInput = new PublishEventsInput(saveResult.Events);
        await Workflow.ExecuteActivityAsync(
            (IWorkflowOrchestrationActivities activities) => activities.PublishApplicationEventsAsync(publishInput),
            activityOptions);
    }
}
