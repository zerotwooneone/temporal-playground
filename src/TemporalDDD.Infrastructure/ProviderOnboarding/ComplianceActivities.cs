using Temporalio.Activities;
using TemporalDDD.Application.ProviderOnboarding;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Infrastructure.Testing;

namespace TemporalDDD.Infrastructure.ProviderOnboarding;

public class ComplianceActivities : IComplianceActivities
{
    private readonly ChaosHttpClient _chaosHttpClient;

    public ComplianceActivities(ChaosHttpClient chaosHttpClient)
    {
        _chaosHttpClient = chaosHttpClient;
    }

    [Activity]
    public async Task<EvaluationStatus> PerformComplianceCheck(string licenseNumber)
    {
        // Simulate external API call with chaos (100ms latency, 10% failure rate)
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        var response = await _chaosHttpClient.GetAsync($"/api/medical-board/{licenseNumber}");

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
