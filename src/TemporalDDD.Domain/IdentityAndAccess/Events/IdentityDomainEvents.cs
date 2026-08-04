using TemporalDDD.Domain.SeedWork;

namespace TemporalDDD.Domain.IdentityAndAccess.Events;

public sealed record UserCreated(
    UserId UserId,
    UserPublicId UserPublicId,
    string Username,
    string Email) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

public sealed record RoleAssignedToUser(
    UserId UserId,
    RoleId RoleId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
