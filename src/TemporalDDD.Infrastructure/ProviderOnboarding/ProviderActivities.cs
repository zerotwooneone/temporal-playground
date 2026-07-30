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

    public async Task ActivateProvider(ProviderId providerId, EvaluationStatus status)
    {
        var providerProfileId = ProviderProfileId.Create(providerId.Value).Value!;
        var providerProfile = await _providerProfileRepository.GetByIdAsync(providerProfileId);
        
        if (providerProfile == null)
        {
            // Create new provider profile if it doesn't exist
            var firstNameVo = PersonName.Create("John").Value!;
            var lastNameVo = PersonName.Create("Doe").Value!;
            var emailVo = Email.Create("john.doe@example.com").Value!;
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
