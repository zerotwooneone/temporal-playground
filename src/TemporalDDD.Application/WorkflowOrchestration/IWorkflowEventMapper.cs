using TemporalDDD.Application.Messaging;
using TemporalDDD.Domain.SeedWork;

namespace TemporalDDD.Application.WorkflowOrchestration;

public interface IWorkflowEventMapper
{
    IApplicationEvent MapToApplicationEvent(IDomainEvent domainEvent);
}
