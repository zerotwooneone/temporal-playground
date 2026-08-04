using Temporalio.Activities;

namespace TemporalDDD.Application.WorkflowOrchestration;

public interface IWorkflowOrchestrationActivities
{
    [Activity]
    Task<SaveWorkflowResult> CreateDraftAndSaveAsync(CreateWorkflowDraftInput input);

    [Activity]
    Task PublishApplicationEventsAsync(PublishEventsInput input);
}
