using Temporalio.Activities;
using TemporalDDD.Application.ProviderOnboarding;
using TemporalDDD.Domain.ProviderOnboarding;

namespace TemporalDDD.Infrastructure.ProviderOnboarding;

public class ComplianceActivities : IComplianceActivities
{
    [Activity]
    public async Task<ComplianceStatus> PerformComplianceCheck(string licenseNumber)
    {
        // Simulate external API call
        await Task.Delay(100);

        var evaluation = new CredentialEvaluation(licenseNumber);
        
        // Simulate license validation (in real scenario, this would call an external API)
        bool isLicenseValid = licenseNumber.Length > 5;
        
        return evaluation.EvaluateReport(isLicenseValid);
    }
}
