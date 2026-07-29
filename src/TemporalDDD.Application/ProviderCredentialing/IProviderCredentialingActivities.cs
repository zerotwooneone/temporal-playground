using Temporalio.Activities;

namespace TemporalDDD.Application.ProviderCredentialing;

public interface IProviderCredentialingActivities
{
    Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(string licenseNumber, string medicalBoard);
    Task EvaluateAndSaveComplianceAsync(uint evaluationId, MedicalBoardLicenseInfo licenseInfo);
    Task RequestManualReviewAsync(uint evaluationId);
    Task ActivateProviderProfileAsync(uint providerId);
}

public record MedicalBoardLicenseInfo(
    string LicenseNumber,
    string MedicalBoard,
    DateTime ExpiryDate,
    bool IsValid,
    uint ProviderId,
    string? Notes = null
);
