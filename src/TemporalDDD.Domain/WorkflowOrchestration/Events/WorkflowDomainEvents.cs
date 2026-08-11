using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.SeedWork;

namespace TemporalDDD.Domain.WorkflowOrchestration.Events;

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

public sealed record WorkflowInstanceStarted(
    WorkflowInstancePublicId WorkflowInstancePublicId,
    BusinessReferenceId BusinessReferenceId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
