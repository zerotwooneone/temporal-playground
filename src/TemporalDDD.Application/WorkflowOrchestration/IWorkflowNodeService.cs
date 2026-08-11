using TemporalDDD.Domain.WorkflowOrchestration;

namespace TemporalDDD.Application.WorkflowOrchestration;

public interface IWorkflowNodeService
{
    Task UpdateNodesAsync(WorkflowDefinitionId workflowDefinitionId, string flowJson, UpdateWorkflowNodesInput input, CancellationToken cancellationToken = default);
}
