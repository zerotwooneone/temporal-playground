using Temporalio.Activities;
using TemporalDDD.Application.ProviderOnboarding;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Infrastructure.ProviderOnboarding;

public class ProviderActivities : IProviderActivities
{
    private readonly IProviderProfileRepository _providerProfileRepository;

    public ProviderActivities(IProviderProfileRepository providerProfileRepository)
    {
        _providerProfileRepository = providerProfileRepository;
    }

    [Activity]
    public async Task ActivateProvider(uint providerId, EvaluationStatus status)
    {
        var providerIdVo = ProviderProfileId.Create(providerId);
        var providerProfile = await _providerProfileRepository.GetByIdAsync(providerIdVo);
        
        if (providerProfile == null)
        {
            // Create new provider profile if it doesn't exist
            var firstNameVo = PersonName.Create("John");
            var lastNameVo = PersonName.Create("Doe");
            var emailVo = Email.Create("john.doe@example.com");
            var specialtyVo = Specialty.Cardiology;
            
            providerProfile = ProviderProfile.Create(firstNameVo, lastNameVo, emailVo, specialtyVo);
        }
        
        if (status == EvaluationStatus.Approved)
        {
            providerProfile.Activate();
            await _providerProfileRepository.SaveAsync(providerProfile);
        }
    }
}
