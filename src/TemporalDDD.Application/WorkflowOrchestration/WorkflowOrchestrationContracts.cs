using TemporalDDD.Application.Messaging;

namespace TemporalDDD.Application.WorkflowOrchestration;

public record CreateWorkflowDraftInput(
    string CreatorId,
    string Name);

public record SaveWorkflowResult(
    string WorkflowId,
    IReadOnlyList<IApplicationEvent> Events);

public record PublishEventsInput(
    IReadOnlyList<IApplicationEvent> Events);
