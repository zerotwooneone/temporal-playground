namespace TemporalDDD.Domain.IdentityAndAccess;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id, CancellationToken cancellationToken = default);
    Task SaveAsync(User aggregate, CancellationToken cancellationToken = default);
}
