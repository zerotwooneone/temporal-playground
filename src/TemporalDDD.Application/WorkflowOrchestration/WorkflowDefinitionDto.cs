namespace TemporalDDD.Application.WorkflowOrchestration;

public record WorkflowDefinitionDto(
    string Id,
    string PublicId,
    string Name,
    string Status,
    int NodeCount
);
