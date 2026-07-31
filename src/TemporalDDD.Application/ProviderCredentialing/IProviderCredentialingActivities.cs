using Temporalio.Activities;
using TemporalDDD.Domain.ProviderCredentialing;

namespace TemporalDDD.Application.ProviderCredentialing;

public interface IProviderCredentialingActivities
{
    [Activity]
    Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(FetchLicenseInput input);
    [Activity]
    Task<EvaluateComplianceResult> EvaluateAndSaveComplianceAsync(EvaluateComplianceInput input);
    [Activity]
    Task RequestManualReviewAsync(RequestManualReviewInput input);
    [Activity]
    Task<string> GetOrCreateProviderProfileAsync(GetOrCreateProviderProfileInput input);
    [Activity]
    Task ActivateProviderProfileAsync(ActivateProviderProfileInput input);
    [Activity]
    Task UpdateEvaluationStatusAsync(UpdateEvaluationStatusInput input);
}

// Primitive DTOs for activity parameters
public record FetchLicenseInput(string LicenseNumber, string MedicalBoard);
public record EvaluateComplianceInput(string ProviderId, string LicenseNumber, string MedicalBoard, DateTimeOffset ExpiryDate, bool IsValid, string ProviderIdResult, string? Notes = null);
public record RequestManualReviewInput(string EvaluationId);
public record GetOrCreateProviderProfileInput(string ProviderId, string FirstName, string LastName, string Email, string Specialty);
public record ActivateProviderProfileInput(string ProviderProfileId);

// Primitive result types
public record MedicalBoardLicenseInfo(
    string LicenseNumber,
    string MedicalBoard,
    DateTimeOffset ExpiryDate,
    bool IsValid,
    string ProviderId,
    string? Notes = null
);

public record EvaluateComplianceResult(
    string EvaluationId,
    bool IsValid,
    bool IsCompliant
);

public record UpdateEvaluationStatusInput(string EvaluationId, bool IsCompliant, string? Notes = null);
