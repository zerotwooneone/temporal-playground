using TemporalDDD.Application.Messaging;
using TemporalDDD.Domain.SeedWork;

namespace TemporalDDD.Application.ProviderCredentialing;

public interface ICredentialEvaluationEventMapper
{
    IApplicationEvent MapToApplicationEvent(IDomainEvent domainEvent);

    IReadOnlyCollection<IApplicationEvent> MapToApplicationEvents(IEnumerable<IDomainEvent> domainEvents)
    {
        return domainEvents.Select(MapToApplicationEvent).ToList();
    }
}
