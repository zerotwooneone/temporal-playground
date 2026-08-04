namespace TemporalDDD.Domain.WorkflowOrchestration;

public interface IWorkflowDefinitionRepository
{
    Task<WorkflowDefinition?> GetByIdAsync(WorkflowDefinitionId id, CancellationToken cancellationToken = default);
    Task SaveAsync(WorkflowDefinition aggregate, CancellationToken cancellationToken = default);
}
