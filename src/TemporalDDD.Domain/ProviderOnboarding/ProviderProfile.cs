using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderOnboarding;

public class ProviderProfile
{
    public uint Id { get; private set; }
    public ProviderPublicId? PublicId { get; private set; }
    public ProviderId ProviderId { get; private set; }
    public bool IsActive { get; private set; }

    private ProviderProfile() { }

    // Factory for creating new profile (ID will be set by database)
    public static ProviderProfile Create(ProviderId providerId)
    {
        return new ProviderProfile
        {
            Id = 0, // Temporary, will be set by DB
            PublicId = ProviderPublicId.New(),
            ProviderId = providerId,
            IsActive = false
        };
    }

    // Factory for rehydrating from database
    public static ProviderProfile FromDatabase(uint id, Guid? publicId, ProviderId providerId, bool isActive)
    {
        return new ProviderProfile
        {
            Id = id,
            PublicId = publicId.HasValue ? ProviderPublicId.Create(publicId.Value) : null,
            ProviderId = providerId,
            IsActive = isActive
        };
    }

    public void Activate()
    {
        IsActive = true;
    }
}
