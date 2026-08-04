namespace TemporalDDD.Application.WorkflowOrchestration;

public interface IWorkflowDefinitionQuery
{
    Task<IReadOnlyList<WorkflowDefinitionDto>> GetAllWorkflowsAsync(CancellationToken cancellationToken = default);
}
