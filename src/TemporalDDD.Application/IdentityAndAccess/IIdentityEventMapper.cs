using TemporalDDD.Application.Messaging;
using TemporalDDD.Domain.SeedWork;

namespace TemporalDDD.Application.IdentityAndAccess;

public interface IIdentityEventMapper
{
    IApplicationEvent MapToApplicationEvent(IDomainEvent domainEvent);
}
