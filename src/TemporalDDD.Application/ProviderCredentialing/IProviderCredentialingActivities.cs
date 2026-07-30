using Temporalio.Activities;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Application.ProviderCredentialing;

public interface IProviderCredentialingActivities
{
    [Activity]
    Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(LicenseNumber licenseNumber, MedicalBoard medicalBoard);
    [Activity]
    Task<CredentialEvaluationId> EvaluateAndSaveComplianceAsync(ProviderId providerId, MedicalBoardLicenseInfo licenseInfo);
    [Activity]
    Task RequestManualReviewAsync(CredentialEvaluationId evaluationId);
    [Activity]
    Task ActivateProviderProfileAsync(ProviderProfileId providerId);
}

public record MedicalBoardLicenseInfo(
    LicenseNumber LicenseNumber,
    MedicalBoard MedicalBoard,
    LicenseExpiryDate ExpiryDate,
    bool IsValid,
    ProviderId ProviderId,
    string? Notes = null
);
