using Temporalio.Activities;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Application.ProviderCredentialing;

public interface IProviderCredentialingActivities
{
    Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(LicenseNumber licenseNumber, MedicalBoard medicalBoard);
    Task<CredentialEvaluationId> EvaluateAndSaveComplianceAsync(ProviderId providerId, MedicalBoardLicenseInfo licenseInfo);
    Task RequestManualReviewAsync(CredentialEvaluationId evaluationId);
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
