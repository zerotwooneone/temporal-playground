using Temporalio.Workflows;

namespace TemporalDDD.Application.WorkflowOrchestration;

[Workflow]
public class UpdateWorkflowNodesWorkflow
{
    private readonly IWorkflowOrchestrationActivities _activities;

    public UpdateWorkflowNodesWorkflow(IWorkflowOrchestrationActivities activities)
    {
        _activities = activities;
    }

    [WorkflowRun]
    public async Task RunAsync(UpdateWorkflowNodesInput input)
    {
        var activityOptions = new ActivityOptions
        {
            StartToCloseTimeout = TimeSpan.FromMinutes(5)
        };

        // Execute UpdateNodesAndSaveAsync activity
        var saveResult = await Workflow.ExecuteActivityAsync(
            () => _activities.UpdateNodesAndSaveAsync(input),
            activityOptions);

        // Execute PublishApplicationEventsAsync activity with the events from the first activity
        var publishInput = new PublishEventsInput(saveResult.Events);
        await Workflow.ExecuteActivityAsync(
            () => _activities.PublishApplicationEventsAsync(publishInput),
            activityOptions);
    }
}
