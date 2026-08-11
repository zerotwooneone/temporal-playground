using TemporalDDD.Domain.WorkflowOrchestration;

namespace TemporalDDD.Application.WorkflowOrchestration;

public interface IWorkflowDefinitionQuery
{
    Task<IReadOnlyList<WorkflowDefinitionDto>> GetAllWorkflowsAsync(CancellationToken cancellationToken = default);
    Task<WorkflowDetailDto?> GetWorkflowByPublicIdAsync(WorkflowDefinitionPublicId publicId, CancellationToken cancellationToken = default);
}
