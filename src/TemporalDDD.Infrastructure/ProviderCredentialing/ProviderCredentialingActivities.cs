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

    [Activity]
    public async Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(FetchLicenseInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var licenseNumberResult = LicenseNumber.Create(input.LicenseNumber);
        if (licenseNumberResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid LicenseNumber. {licenseNumberResult.Error}");
        
        var medicalBoardResult = MedicalBoard.Create(input.MedicalBoard);
        if (medicalBoardResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid MedicalBoard. {medicalBoardResult.Error}");

        var licenseNumber = licenseNumberResult.Value;
        var medicalBoard = medicalBoardResult.Value;

        // Simulate external API call to medical board with chaos (100ms latency, 10% failure rate)
        var response = await _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError)
            .GetAsync($"https://external-medical-board.example.com/api/{medicalBoard.Value}/license/{licenseNumber.Value}");

        // Simulated response - in real implementation, this would call actual medical board API
        var isValid = licenseNumber.Value.Length >= 8;
        var expiryDateResult = LicenseExpiryDate.Create(DateTimeOffset.UtcNow.AddYears(2));
        var providerIdResult = ProviderId.Create(1);

        return new MedicalBoardLicenseInfo(
            LicenseNumber: licenseNumber.Value,
            MedicalBoard: medicalBoard.Value,
            ExpiryDate: expiryDateResult.Value.Value,
            IsValid: isValid,
            ProviderId: providerIdResult.Value.Value,
            Notes: isValid ? "License verified successfully" : "License number format invalid"
        );
    }

    [Activity]
    public async Task<CredentialEvaluationId> EvaluateAndSaveComplianceAsync(EvaluateComplianceInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var providerIdResult = ProviderId.Create(input.ProviderId);
        if (providerIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid ProviderId. {providerIdResult.Error}");
        
        var licenseNumberResult = LicenseNumber.Create(input.LicenseNumber);
        if (licenseNumberResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid LicenseNumber. {licenseNumberResult.Error}");
        
        var medicalBoardResult = MedicalBoard.Create(input.MedicalBoard);
        if (medicalBoardResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid MedicalBoard. {medicalBoardResult.Error}");
        
        var expiryDateResult = LicenseExpiryDate.Create(input.ExpiryDate);
        if (expiryDateResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid ExpiryDate. {expiryDateResult.Error}");
        
        var providerIdResult2 = ProviderId.Create(input.ProviderIdResult);
        if (providerIdResult2.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid ProviderIdResult. {providerIdResult2.Error}");

        var providerId = providerIdResult.Value;
        var licenseNumber = licenseNumberResult.Value;
        var medicalBoard = medicalBoardResult.Value;
        var expiryDate = expiryDateResult.Value;
        var providerIdResultValue = providerIdResult2.Value;

        // Reconstruct MedicalBoardLicenseInfo from primitive DTO (now using primitives)
        var licenseInfo = new MedicalBoardLicenseInfo(
            LicenseNumber: input.LicenseNumber,
            MedicalBoard: input.MedicalBoard,
            ExpiryDate: input.ExpiryDate,
            IsValid: input.IsValid,
            ProviderId: providerIdResultValue,
            Notes: input.Notes
        );

        // Simulate business rule evaluation
        var isCompliant = licenseInfo.IsValid && licenseInfo.ExpiryDate > DateTimeOffset.UtcNow.AddMonths(6);

        // Convert primitives back to domain types for entity creation
        var licenseNumberForEntity = LicenseNumber.Create(licenseInfo.LicenseNumber).Value!;
        var medicalBoardForEntity = MedicalBoard.Create(licenseInfo.MedicalBoard).Value!;
        var expiryDateForEntity = LicenseExpiryDate.Create(licenseInfo.ExpiryDate).Value!;

        // Create domain entity using factory
        var evaluation = Domain.ProviderCredentialing.CredentialEvaluation.Create(
            providerId: providerId,
            licenseNumber: licenseNumberForEntity,
            medicalBoard: medicalBoardForEntity,
            licenseExpiryDate: expiryDateForEntity
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

    [Activity]
    public async Task RequestManualReviewAsync(RequestManualReviewInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var evaluationIdResult = CredentialEvaluationId.Create(input.EvaluationId);
        if (evaluationIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid CredentialEvaluationId. {evaluationIdResult.Error}");

        var evaluationId = evaluationIdResult.Value;

        // Simulate external notification (e.g., email, webhook) with chaos
        _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError);

        await _chaosHttpClient.PostAsJsonAsync($"https://notifications.example.com/api/manual-review/{evaluationId.Value}", new { });

        Console.WriteLine($"[ManualReview] Request sent for evaluation {evaluationId.Value}");
    }

    [Activity]
    public async Task ActivateProviderProfileAsync(ActivateProviderProfileInput input)
    {
        // Convert primitive DTO to Domain types with fail-fast validation
        var providerProfileIdResult = ProviderProfileId.Create(input.ProviderProfileId);
        if (providerProfileIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid ProviderProfileId. {providerProfileIdResult.Error}");

        var providerProfileId = providerProfileIdResult.Value;

        var providerProfile = await _providerProfileRepository.GetByIdAsync(providerProfileId);

        if (providerProfile == null)
        {
            throw new InvalidOperationException($"Provider profile {providerProfileId.Value} not found");
        }

        providerProfile.Activate();
        await _providerProfileRepository.SaveAsync(providerProfile);

        Console.WriteLine($"[ProviderActivation] Provider {providerProfileId.Value} activated successfully");
    }
}
