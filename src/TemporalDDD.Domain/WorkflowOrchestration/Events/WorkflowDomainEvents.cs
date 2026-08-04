using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.SeedWork;

namespace TemporalDDD.Domain.WorkflowOrchestration.Events;

public sealed record WorkflowDraftCreated(
    WorkflowDefinitionId WorkflowDefinitionId,
    WorkflowDefinitionPublicId WorkflowDefinitionPublicId,
    UserId CreatorId,
    string Name) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

public sealed record WorkflowSubmittedForReview(
    WorkflowDefinitionId WorkflowDefinitionId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

public sealed record WorkflowApproved(
    WorkflowDefinitionId WorkflowDefinitionId,
    UserId ReviewerId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

public sealed record WorkflowRejected(
    WorkflowDefinitionId WorkflowDefinitionId,
    UserId ReviewerId,
    string Reason) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
