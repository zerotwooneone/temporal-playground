using TemporalDDD.Application.IdentityAndAccess;
using TemporalDDD.Application.Messaging;
using TemporalDDD.Domain.IdentityAndAccess.Events;
using TemporalDDD.Domain.SeedWork;

namespace TemporalDDD.Infrastructure.IdentityAndAccess;

public class IdentityEventMapper : IIdentityEventMapper
{
    public IApplicationEvent MapToApplicationEvent(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            Domain.IdentityAndAccess.Events.UserCreated e =>
                new Application.IdentityAndAccess.UserCreatedEvent(
                    UserId: e.UserId.ToString(),
                    PublicId: e.UserPublicId.ToString(),
                    Username: e.Username,
                    Email: e.Email),

            Domain.IdentityAndAccess.Events.RoleAssignedToUser e =>
                new Application.Messaging.UnknownTypeEvent(
                    new InvalidOperationException($"RoleAssignedToUser event not yet mapped to application event").ToString()),

            _ => new Application.Messaging.UnknownTypeEvent(
                new InvalidOperationException($"Unknown domain event type: {domainEvent.GetType().Name}").ToString())
        };
    }
}
