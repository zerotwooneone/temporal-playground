using Temporalio.Activities;
using TemporalDDD.Application.ProviderOnboarding;
using TemporalDDD.Domain.ProviderOnboarding;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;

namespace TemporalDDD.Infrastructure.ProviderOnboarding;

public class ComplianceActivities : IComplianceActivities
{
    [Activity]
    public async Task<ComplianceStatus> PerformComplianceCheck(string licenseNumber)
    {
        // Simulate external API call
        await Task.Delay(100);

        var licenseNumberVo = LicenseNumber.Create(licenseNumber);
        var evaluation = new CredentialEvaluation(licenseNumberVo);
        
        // Simulate license validation (in real scenario, this would call an external API)
        bool isLicenseValid = licenseNumber.Length > 5;
        
        return evaluation.EvaluateReport(isLicenseValid);
    }
}
