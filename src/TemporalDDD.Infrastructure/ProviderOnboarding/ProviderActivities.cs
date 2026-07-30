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
    public async Task ActivateProvider(ActivateProviderInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var providerIdResult = ProviderId.Create(input.ProviderId);
        if (providerIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid ProviderId. {providerIdResult.Error}");
        
        var statusResult = EvaluationStatus.FromValue(input.Status);
        if (statusResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid EvaluationStatus. {statusResult.Error}");

        var providerId = providerIdResult.Value;
        var status = statusResult.Value;

        var hackyConversion = ProviderProfileId.Abbreviation + providerId.Value;
        var providerProfileId = ProviderProfileId.Create(hackyConversion).Value!;
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
