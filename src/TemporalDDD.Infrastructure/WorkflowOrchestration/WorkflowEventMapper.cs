using TemporalDDD.Application.Messaging;
using TemporalDDD.Application.WorkflowOrchestration;
using TemporalDDD.Domain.SeedWork;
using TemporalDDD.Domain.WorkflowOrchestration.Events;

namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public class WorkflowEventMapper : IWorkflowEventMapper
{
    public IApplicationEvent MapToApplicationEvent(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            Domain.WorkflowOrchestration.Events.WorkflowDraftCreated e =>
                new Application.WorkflowOrchestration.WorkflowDraftCreatedEvent(
                    WorkflowId: e.WorkflowDefinitionId.ToString(),
                    PublicId: e.WorkflowDefinitionPublicId.ToString(),
                    CreatorId: e.CreatorId.ToString(),
                    Name: e.Name),

            Domain.WorkflowOrchestration.Events.WorkflowSubmittedForReview e =>
                new Application.Messaging.UnknownTypeEvent(
                    new InvalidOperationException($"WorkflowSubmittedForReview event not yet mapped to application event").ToString()),

            Domain.WorkflowOrchestration.Events.WorkflowApproved e =>
                new Application.Messaging.UnknownTypeEvent(
                    new InvalidOperationException($"WorkflowApproved event not yet mapped to application event").ToString()),

            Domain.WorkflowOrchestration.Events.WorkflowRejected e =>
                new Application.Messaging.UnknownTypeEvent(
                    new InvalidOperationException($"WorkflowRejected event not yet mapped to application event").ToString()),

            _ => new Application.Messaging.UnknownTypeEvent(
                new InvalidOperationException($"Unknown domain event type: {domainEvent.GetType().Name}").ToString())
        };
    }
}
