using Temporalio.Activities;
using TemporalDDD.Application.ProviderOnboarding;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Infrastructure.ProviderOnboarding;

public class ComplianceActivities : IComplianceActivities
{
    [Activity]
    public async Task<EvaluationStatus> PerformComplianceCheck(string licenseNumber)
    {
        // Simulate external API call
        await Task.Delay(100);

        var licenseNumberVo = LicenseNumber.Create(licenseNumber);
        var providerIdVo = ProviderId.Create(1); // Simulated provider ID
        var medicalBoardVo = MedicalBoard.Create("Default");
        var licenseExpiryDateVo = LicenseExpiryDate.Create(DateTimeOffset.UtcNow.AddYears(2));
        
        var evaluation = CredentialEvaluation.Create(providerIdVo, licenseNumberVo, medicalBoardVo, licenseExpiryDateVo);
        
        // Simulate license validation (in real scenario, this would call an external API)
        bool isLicenseValid = licenseNumber.Length > 5;
        
        if (isLicenseValid)
        {
            evaluation.MarkAsCompliant("License verified successfully");
        }
        else
        {
            evaluation.MarkAsNonCompliant("License number format invalid");
        }
        
        return evaluation.Status;
    }
}
