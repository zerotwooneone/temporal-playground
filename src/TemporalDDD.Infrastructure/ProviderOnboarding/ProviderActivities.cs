using Temporalio.Activities;
using TemporalDDD.Application.ProviderOnboarding;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Infrastructure.ProviderOnboarding;

public class ProviderActivities : IProviderActivities
{
    [Activity]
    public async Task ActivateProvider(uint providerId, EvaluationStatus status)
    {
        // Simulate database save
        await Task.Delay(100);

        var providerIdVo = ProviderId.Create(providerId);
        var firstNameVo = PersonName.Create("John");
        var lastNameVo = PersonName.Create("Doe");
        var emailVo = Email.Create("john.doe@example.com");
        var specialtyVo = Specialty.Cardiology;
        
        var providerProfile = ProviderProfile.Create(firstNameVo, lastNameVo, emailVo, specialtyVo);
        
        if (status == EvaluationStatus.Approved)
        {
            providerProfile.Activate();
            
            // In real scenario, save to database here
            // await _repository.SaveAsync(providerProfile);
        }
    }
}
