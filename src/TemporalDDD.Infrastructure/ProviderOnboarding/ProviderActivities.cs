using Temporalio.Activities;
using TemporalDDD.Application.ProviderOnboarding;
using TemporalDDD.Domain.ProviderOnboarding;

namespace TemporalDDD.Infrastructure.ProviderOnboarding;

public class ProviderActivities : IProviderActivities
{
    [Activity]
    public async Task ActivateProvider(string providerId, ComplianceStatus status)
    {
        // Simulate database save
        await Task.Delay(100);

        var providerProfile = new ProviderProfile(providerId);
        
        if (status == ComplianceStatus.Cleared)
        {
            providerProfile.Activate();
            
            // In real scenario, save to database here
            // await _repository.SaveAsync(providerProfile);
        }
    }
}
