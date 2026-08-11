using Temporalio.Activities;

namespace TemporalDDD.Application.WorkflowOrchestration;

public interface IWorkflowOrchestrationActivities
{
    [Activity]
    Task<SaveWorkflowResult> UpdateNodesAndSaveAsync(UpdateWorkflowNodesInput input);

    [Activity]
    Task PublishApplicationEventsAsync(PublishEventsInput input);
}
