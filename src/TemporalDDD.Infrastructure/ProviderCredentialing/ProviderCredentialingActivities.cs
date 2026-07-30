using Microsoft.EntityFrameworkCore;
using Temporalio.Activities;
using TemporalDDD.Application.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Infrastructure.Testing;

namespace TemporalDDD.Infrastructure.ProviderCredentialing;

public class ProviderCredentialingActivities : IProviderCredentialingActivities
{
    private readonly ICredentialEvaluationRepository _credentialEvaluationRepository;
    private readonly IProviderProfileRepository _providerProfileRepository;
    private readonly ChaosHttpClient _chaosHttpClient;

    public ProviderCredentialingActivities(
        ICredentialEvaluationRepository credentialEvaluationRepository,
        IProviderProfileRepository providerProfileRepository,
        ChaosHttpClient chaosHttpClient)
    {
        _credentialEvaluationRepository = credentialEvaluationRepository;
        _providerProfileRepository = providerProfileRepository;
        _chaosHttpClient = chaosHttpClient;
    }

    public async Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(LicenseNumber licenseNumber, MedicalBoard medicalBoard)
    {
        // Simulate external API call to medical board with chaos (100ms latency, 10% failure rate)
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        var response = await _chaosHttpClient.GetAsync($"/api/medical-board/{medicalBoard.Value}/license/{licenseNumber.Value}");

        // Simulated response - in real implementation, this would call actual medical board API
        var isValid = licenseNumber.Value.Length >= 8;
        var expiryDate = LicenseExpiryDate.Create(DateTimeOffset.UtcNow.AddYears(2));

        return new MedicalBoardLicenseInfo(
            LicenseNumber: licenseNumber,
            MedicalBoard: medicalBoard,
            ExpiryDate: expiryDate,
            IsValid: isValid,
            ProviderId: ProviderId.Create(1), // Simulated provider ID
            Notes: isValid ? "License verified successfully" : "License number format invalid"
        );
    }

    public async Task<CredentialEvaluationId> EvaluateAndSaveComplianceAsync(ProviderId providerId, MedicalBoardLicenseInfo licenseInfo)
    {
        // Simulate business rule evaluation
        var isCompliant = licenseInfo.IsValid && licenseInfo.ExpiryDate.Value > DateTimeOffset.UtcNow.AddMonths(6);

        // Create domain entity using factory
        var evaluation = Domain.ProviderCredentialing.CredentialEvaluation.Create(
            providerId: providerId,
            licenseNumber: licenseInfo.LicenseNumber,
            medicalBoard: licenseInfo.MedicalBoard,
            licenseExpiryDate: licenseInfo.ExpiryDate
        );

        if (isCompliant)
        {
            evaluation.MarkAsCompliant(licenseInfo.Notes);
        }
        else
        {
            evaluation.RequestManualReview();
        }

        // Save to database using repository
        await _credentialEvaluationRepository.SaveAsync(evaluation);

        return evaluation.Id;
    }

    public async Task RequestManualReviewAsync(CredentialEvaluationId evaluationId)
    {
        // Simulate external notification (e.g., email, webhook) with chaos
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        await _chaosHttpClient.PostAsJsonAsync($"/api/notifications/manual-review/{evaluationId.Value}", new { });

        Console.WriteLine($"[ManualReview] Request sent for evaluation {evaluationId.Value}");
    }

    public async Task ActivateProviderProfileAsync(ProviderProfileId providerId)
    {
        var providerProfile = await _providerProfileRepository.GetByIdAsync(providerId);

        if (providerProfile == null)
        {
            throw new InvalidOperationException($"Provider profile {providerId.Value} not found");
        }

        providerProfile.Activate();
        await _providerProfileRepository.SaveAsync(providerProfile);

        Console.WriteLine($"[ProviderActivation] Provider {providerId.Value} activated successfully");
    }
}
