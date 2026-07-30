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

    public async Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(string licenseNumber, string medicalBoard)
    {
        // Simulate external API call to medical board with chaos (100ms latency, 10% failure rate)
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        var response = await _chaosHttpClient.GetAsync($"/api/medical-board/{medicalBoard}/license/{licenseNumber}");

        // Simulated response - in real implementation, this would call actual medical board API
        var isValid = !string.IsNullOrEmpty(licenseNumber) && licenseNumber.Length >= 8;
        var expiryDate = DateTimeOffset.UtcNow.AddYears(2);

        return new MedicalBoardLicenseInfo(
            LicenseNumber: licenseNumber,
            MedicalBoard: medicalBoard,
            ExpiryDate: expiryDate,
            IsValid: isValid,
            ProviderId: 1, // Simulated provider ID
            Notes: isValid ? "License verified successfully" : "License number format invalid"
        );
    }

    public async Task EvaluateAndSaveComplianceAsync(uint evaluationId, MedicalBoardLicenseInfo licenseInfo)
    {
        // Simulate business rule evaluation
        var isCompliant = licenseInfo.IsValid && licenseInfo.ExpiryDate > DateTimeOffset.UtcNow.AddMonths(6);

        // Create value objects
        var licenseNumberVo = LicenseNumber.Create(licenseInfo.LicenseNumber);
        var medicalBoardVo = MedicalBoard.Create(licenseInfo.MedicalBoard);
        var licenseExpiryDateVo = LicenseExpiryDate.Create(licenseInfo.ExpiryDate);
        var providerIdVo = ProviderId.Create(licenseInfo.ProviderId);

        // Create domain entity using factory
        var evaluation = Domain.ProviderCredentialing.CredentialEvaluation.Create(
            providerId: providerIdVo,
            licenseNumber: licenseNumberVo,
            medicalBoard: medicalBoardVo,
            licenseExpiryDate: licenseExpiryDateVo
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
    }

    public async Task RequestManualReviewAsync(uint evaluationId)
    {
        // Simulate external notification (e.g., email, webhook) with chaos
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        await _chaosHttpClient.PostAsJsonAsync($"/api/notifications/manual-review/{evaluationId}", new { });

        Console.WriteLine($"[ManualReview] Request sent for evaluation {evaluationId}");
    }

    public async Task ActivateProviderProfileAsync(uint providerId)
    {
        var providerIdVo = ProviderProfileId.Create(providerId);
        var providerProfile = await _providerProfileRepository.GetByIdAsync(providerIdVo);

        if (providerProfile == null)
        {
            throw new InvalidOperationException($"Provider profile {providerId} not found");
        }

        providerProfile.Activate();
        await _providerProfileRepository.SaveAsync(providerProfile);

        Console.WriteLine($"[ProviderActivation] Provider {providerId} activated successfully");
    }
}
