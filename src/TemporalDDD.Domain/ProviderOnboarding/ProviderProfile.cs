using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderOnboarding;

public class ProviderProfile
{
    public ProviderId ProviderId { get; }
    public bool IsActive { get; private set; }

    public ProviderProfile(ProviderId providerId)
    {
        ProviderId = providerId;
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
