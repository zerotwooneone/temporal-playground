using Temporalio.Workflows;

namespace TemporalDDD.Application.WorkflowOrchestration;

[Workflow]
public class CreateWorkflowDraftWorkflow
{
    private readonly IWorkflowOrchestrationActivities _activities;

    public CreateWorkflowDraftWorkflow(IWorkflowOrchestrationActivities activities)
    {
        _activities = activities;
    }

    [WorkflowRun]
    public async Task RunAsync(CreateWorkflowDraftInput input)
    {
        var activityOptions = new ActivityOptions
        {
            StartToCloseTimeout = TimeSpan.FromMinutes(5)
        };

        // Execute CreateDraftAndSaveAsync activity
        var saveResult = await Workflow.ExecuteActivityAsync(
            () => _activities.CreateDraftAndSaveAsync(input),
            activityOptions);

        // Execute PublishApplicationEventsAsync activity with the events from the first activity
        var publishInput = new PublishEventsInput(saveResult.Events);
        await Workflow.ExecuteActivityAsync(
            () => _activities.PublishApplicationEventsAsync(publishInput),
            activityOptions);
    }
}
