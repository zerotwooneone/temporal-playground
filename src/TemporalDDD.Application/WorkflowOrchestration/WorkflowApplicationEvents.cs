using TemporalDDD.Application.Messaging;

namespace TemporalDDD.Application.WorkflowOrchestration;

public sealed record WorkflowDraftCreatedEvent(
    string WorkflowId,
    string PublicId,
    string CreatorId,
    string Name) : IApplicationEvent;
