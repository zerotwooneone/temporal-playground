using TemporalDDD.Domain.WorkflowOrchestration;

namespace TemporalDDD.Application.WorkflowOrchestration;

public interface IWorkflowNodeService
{
    Task UpdateNodesAsync(WorkflowDefinitionId workflowDefinitionId, UpdateWorkflowNodesInput input, CancellationToken cancellationToken = default);
}
