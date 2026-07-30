using Temporalio.Activities;
using TemporalDDD.Domain.ProviderCredentialing;

namespace TemporalDDD.Application.ProviderCredentialing;

public interface IProviderCredentialingActivities
{
    [Activity]
    Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(FetchLicenseInput input);
    [Activity]
    Task<string> EvaluateAndSaveComplianceAsync(EvaluateComplianceInput input);
    [Activity]
    Task RequestManualReviewAsync(RequestManualReviewInput input);
    [Activity]
    Task ActivateProviderProfileAsync(ActivateProviderProfileInput input);
}

// Primitive DTOs for activity parameters
public record FetchLicenseInput(string LicenseNumber, string MedicalBoard);
public record EvaluateComplianceInput(string ProviderId, string LicenseNumber, string MedicalBoard, DateTimeOffset ExpiryDate, bool IsValid, string ProviderIdResult, string? Notes = null);
public record RequestManualReviewInput(string EvaluationId);
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
