using TemporalDDD.Application.Messaging;

namespace TemporalDDD.Application.IdentityAndAccess;

public sealed record UserCreatedEvent(
    string UserId,
    string PublicId,
    string Username,
    string Email) : IApplicationEvent;
