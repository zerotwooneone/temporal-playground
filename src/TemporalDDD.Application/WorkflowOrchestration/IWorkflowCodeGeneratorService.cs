using TemporalDDD.Domain.WorkflowOrchestration;

namespace TemporalDDD.Application.WorkflowOrchestration;

public interface IWorkflowCodeGeneratorService
{
    Task<string> GenerateWorkflowClassAsync(WorkflowDefinition workflowDefinition, string outputDirectoryPath);
}
