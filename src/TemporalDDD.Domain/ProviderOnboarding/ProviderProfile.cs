namespace TemporalDDD.Domain.ProviderOnboarding;

public class ProviderProfile
{
    public string ProviderId { get; }
    public bool IsActive { get; private set; }

    public ProviderProfile(string providerId)
    {
        ProviderId = providerId;
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}
