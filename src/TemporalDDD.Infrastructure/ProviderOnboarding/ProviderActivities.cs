using Temporalio.Activities;
using TemporalDDD.Application.ProviderOnboarding;
using TemporalDDD.Domain.ProviderOnboarding;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Infrastructure.ProviderOnboarding;

public class ProviderActivities : IProviderActivities
{
    [Activity]
    public async Task ActivateProvider(uint providerId, ComplianceStatus status)
    {
        // Simulate database save
        await Task.Delay(100);

        var providerIdVo = ProviderId.Create(providerId);
        var providerProfile = ProviderProfile.Create(providerIdVo);
        
        if (status == ComplianceStatus.Cleared)
        {
            providerProfile.Activate();
            
            // In real scenario, save to database here
            // await _repository.SaveAsync(providerProfile);
        }
    }
}
