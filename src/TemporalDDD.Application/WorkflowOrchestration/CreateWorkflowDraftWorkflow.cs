using Temporalio.Workflows;

namespace TemporalDDD.Application.WorkflowOrchestration;

[Workflow]
public class CreateWorkflowDraftWorkflow
{
    [WorkflowRun]
    public async Task RunAsync(CreateWorkflowDraftInput input)
    {
        var activityOptions = new ActivityOptions
        {
            StartToCloseTimeout = TimeSpan.FromMinutes(5)
        };

        // Execute CreateDraftAndSaveAsync activity
        var saveResult = await Workflow.ExecuteActivityAsync(
            (IWorkflowOrchestrationActivities activities) => activities.CreateDraftAndSaveAsync(input),
            activityOptions);

        // Execute PublishApplicationEventsAsync activity with the events from the first activity
        var publishInput = new PublishEventsInput(saveResult.Events);
        await Workflow.ExecuteActivityAsync(
            (IWorkflowOrchestrationActivities activities) => activities.PublishApplicationEventsAsync(publishInput),
            activityOptions);
    }
}
