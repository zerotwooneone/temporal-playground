using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderCredentialing;

public interface IProviderProfileRepository
{
    Task<ProviderProfile?> GetByIdAsync(ProviderProfileId id, CancellationToken cancellationToken = default);
    Task<ProviderProfile?> GetByProviderIdAsync(ProviderId providerId, CancellationToken cancellationToken = default);
    Task SaveAsync(ProviderProfile aggregate, CancellationToken cancellationToken = default);
}
