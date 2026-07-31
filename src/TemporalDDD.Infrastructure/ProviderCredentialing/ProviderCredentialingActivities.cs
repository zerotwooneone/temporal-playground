using Microsoft.Extensions.DependencyInjection;
using Temporalio.Activities;
using TemporalDDD.Application.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Infrastructure.Testing;

namespace TemporalDDD.Infrastructure.ProviderCredentialing;

public class ProviderCredentialingActivities : IProviderCredentialingActivities
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ChaosHttpClient _chaosHttpClient;

    public ProviderCredentialingActivities(
        IServiceScopeFactory scopeFactory,
        ChaosHttpClient chaosHttpClient)
    {
        _scopeFactory = scopeFactory;
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
        var providerIdResult = ProviderId.New();

        return new MedicalBoardLicenseInfo(
            LicenseNumber: licenseNumber.Value,
            MedicalBoard: medicalBoard.Value,
            ExpiryDate: expiryDateResult.Value.Value,
            IsValid: isValid,
            ProviderId: providerIdResult.ToString(),
            Notes: isValid ? "License verified successfully" : "License number format invalid"
        );
    }

    [Activity]
    public async Task<EvaluateComplianceResult> EvaluateAndSaveComplianceAsync(EvaluateComplianceInput input)
    {
        using var scope = _scopeFactory.CreateScope();
        var credentialEvaluationRepository = scope.ServiceProvider.GetRequiredService<ICredentialEvaluationRepository>();

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
            ProviderId: providerIdResultValue.ToString(),
            Notes: input.Notes
        );

        // Create domain entity using factory
        var licenseNumberForEntity = LicenseNumber.Create(licenseInfo.LicenseNumber).Value!;
        var medicalBoardForEntity = MedicalBoard.Create(licenseInfo.MedicalBoard).Value!;
        var expiryDateForEntity = LicenseExpiryDate.Create(licenseInfo.ExpiryDate).Value!;

        var evaluation = Domain.ProviderCredentialing.CredentialEvaluation.Create(
            providerId: providerId,
            licenseNumber: licenseNumberForEntity,
            medicalBoard: medicalBoardForEntity,
            licenseExpiryDate: expiryDateForEntity
        );

        // Save to database using repository
        await credentialEvaluationRepository.SaveAsync(evaluation);

        // Return evaluation result - let workflow decide what to do with it
        return new EvaluateComplianceResult(
            EvaluationId: evaluation.Id.ToString(),
            IsValid: licenseInfo.IsValid,
            IsCompliant: licenseInfo.IsValid // Simplified: valid = compliant for now
        );
    }

    [Activity]
    public async Task RequestManualReviewAsync(RequestManualReviewInput input)
    {
        using var scope = _scopeFactory.CreateScope();
        var credentialEvaluationRepository = scope.ServiceProvider.GetRequiredService<ICredentialEvaluationRepository>();

        // Convert primitive DTO to Domain types with fail-fast validation
        var evaluationIdResult = CredentialEvaluationId.Create(input.EvaluationId);
        if (evaluationIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid CredentialEvaluationId. {evaluationIdResult.Error}");

        var evaluationId = evaluationIdResult.Value;

        // Get evaluation and update status to ManualReviewRequired with WorkflowId
        var evaluation = await credentialEvaluationRepository.GetByIdAsync(evaluationId);
        if (evaluation == null)
            throw new InvalidOperationException($"Evaluation {evaluationId} not found");

        evaluation.RequestManualReview(input.WorkflowId);
        await credentialEvaluationRepository.SaveAsync(evaluation);

        // Simulate external notification
        await _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError)
            .PostAsJsonAsync($"https://notifications.example.com/api/manual-review/{evaluationId}", new { });

        Console.WriteLine($"[ManualReview] Request sent for evaluation {evaluationId}, workflow {input.WorkflowId}");
    }

    [Activity]
    public async Task UpdateEvaluationStatusAsync(UpdateEvaluationStatusInput input)
    {
        using var scope = _scopeFactory.CreateScope();
        var credentialEvaluationRepository = scope.ServiceProvider.GetRequiredService<ICredentialEvaluationRepository>();

        // Convert primitive DTO to Domain types with fail-fast validation
        var evaluationIdResult = CredentialEvaluationId.Create(input.EvaluationId);
        if (evaluationIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid CredentialEvaluationId. {evaluationIdResult.Error}");

        var evaluationId = evaluationIdResult.Value;

        // Get evaluation and update status
        var evaluation = await credentialEvaluationRepository.GetByIdAsync(evaluationId);
        if (evaluation == null)
            throw new InvalidOperationException($"Evaluation {evaluationId} not found");

        if (input.IsCompliant)
        {
            evaluation.MarkAsCompliant(input.Notes);
        }
        else
        {
            evaluation.MarkAsNonCompliant(input.Notes ?? "Manual review rejected");
        }

        await credentialEvaluationRepository.SaveAsync(evaluation);
    }

    [Activity]
    public async Task<string> GetOrCreateProviderProfileAsync(GetOrCreateProviderProfileInput input)
    {
        using var scope = _scopeFactory.CreateScope();
        var providerProfileRepository = scope.ServiceProvider.GetRequiredService<IProviderProfileRepository>();

        // Convert primitive DTO to Domain types with fail-fast validation
        var providerIdResult = ProviderId.Create(input.ProviderId);
        if (providerIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid ProviderId. {providerIdResult.Error}");

        var firstNameResult = PersonName.Create(input.FirstName);
        if (firstNameResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid FirstName. {firstNameResult.Error}");

        var lastNameResult = PersonName.Create(input.LastName);
        if (lastNameResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid LastName. {lastNameResult.Error}");

        var emailResult = Email.Create(input.Email);
        if (emailResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid Email. {emailResult.Error}");

        var specialtyResult = Specialty.Create(input.Specialty);
        if (specialtyResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid Specialty. {specialtyResult.Error}");

        var providerId = providerIdResult.Value;

        // Try to get existing profile by ProviderId
        var existingProfile = await providerProfileRepository.GetByProviderIdAsync(providerId);
        if (existingProfile != null)
        {
            return existingProfile.Id.ToString();
        }

        // Create new profile
        var newProfile = ProviderProfile.Create(
            providerId: providerId,
            firstName: firstNameResult.Value,
            lastName: lastNameResult.Value,
            email: emailResult.Value,
            specialty: specialtyResult.Value
        );

        await providerProfileRepository.SaveAsync(newProfile);
        return newProfile.Id.ToString();
    }

    [Activity]
    public async Task ActivateProviderProfileAsync(ActivateProviderProfileInput input)
    {
        using var scope = _scopeFactory.CreateScope();
        var providerProfileRepository = scope.ServiceProvider.GetRequiredService<IProviderProfileRepository>();

        // Convert primitive DTO to Domain types with fail-fast validation
        
        var providerProfileIdResult = ProviderProfileId.Create(input.ProviderProfileId);
        if (providerProfileIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid ProviderProfileId. {providerProfileIdResult.Error}");

        var providerProfileId = providerProfileIdResult.Value;

        var providerProfile = await providerProfileRepository.GetByIdAsync(providerProfileId);

        if (providerProfile == null)
        {
            throw new InvalidOperationException($"Provider profile {providerProfileId} not found");
        }

        providerProfile.Activate();
        await providerProfileRepository.SaveAsync(providerProfile);
        
        Console.WriteLine($"[ProviderActivation] Provider {providerProfileId} activated successfully");
    }
}
