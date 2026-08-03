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
    private readonly ITimeProvider _timeProvider;

    public ComplianceActivities(ChaosHttpClient chaosHttpClient, ITimeProvider timeProvider)
    {
        _chaosHttpClient = chaosHttpClient;
        _timeProvider = timeProvider;
    }

    [Activity]
    public async Task<int> PerformComplianceCheck(PerformComplianceInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var licenseNumberResult = LicenseNumber.Create(input.LicenseNumber);
        if (licenseNumberResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid LicenseNumber. {licenseNumberResult.Error}");

        var licenseNumber = licenseNumberResult.Value;

        // Simulate external API call with chaos (100ms latency, 10% failure rate)
        var response = await _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError)
            .GetAsync($"https://external-medical-board.example.com/api/license/{licenseNumber.Value}");
        response.EnsureSuccessStatusCode();

        var providerIdVo = ProviderId.New(); // Simulated provider ID
        var medicalBoardVo = MedicalBoard.Create("Default").Value!;
        var futureDate = _timeProvider.LocalToday.AddYears(2);
        var licenseExpiryDateVo = LicenseExpiryDate.Create(futureDate).Value!;
        
        var evaluation = CredentialEvaluation.Create(providerIdVo, CredentialEvaluationPublicId.New(), licenseNumber, medicalBoardVo, licenseExpiryDateVo);
        
        // Simulate license validation (in real scenario, this would call an external API)
        bool isLicenseValid = licenseNumber.Value.Length > 5;
        
        if (isLicenseValid)
        {
            evaluation.MarkAsCompliant("License verified successfully");
        }
        else
        {
            evaluation.MarkAsNonCompliant("License number format invalid");
        }
        
        return (int)evaluation.Status;
    }
}
