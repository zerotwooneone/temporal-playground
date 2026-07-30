using Temporalio.Activities;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Application.ProviderCredentialing;

public interface IProviderCredentialingActivities
{
    [Activity]
    Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(FetchLicenseInput input);
    [Activity]
    Task<CredentialEvaluationId> EvaluateAndSaveComplianceAsync(EvaluateComplianceInput input);
    [Activity]
    Task RequestManualReviewAsync(RequestManualReviewInput input);
    [Activity]
    Task ActivateProviderProfileAsync(ActivateProviderProfileInput input);
}

// Primitive DTOs for activity parameters
public record FetchLicenseInput(string LicenseNumber, string MedicalBoard);
public record EvaluateComplianceInput(uint ProviderId, string LicenseNumber, string MedicalBoard, DateTimeOffset ExpiryDate, bool IsValid, uint ProviderIdResult, string? Notes = null);
public record RequestManualReviewInput(uint EvaluationId);
public record ActivateProviderProfileInput(uint ProviderProfileId);

// Primitive result types
public record MedicalBoardLicenseInfo(
    string LicenseNumber,
    string MedicalBoard,
    DateTimeOffset ExpiryDate,
    bool IsValid,
    uint ProviderId,
    string? Notes = null
);
