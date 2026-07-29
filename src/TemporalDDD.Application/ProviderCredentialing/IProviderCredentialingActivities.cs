using Temporalio.Activities;

namespace TemporalDDD.Application.ProviderCredentialing;

public interface IProviderCredentialingActivities
{
    Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(string licenseNumber, string medicalBoard);
    Task EvaluateAndSaveComplianceAsync(Guid evaluationId, MedicalBoardLicenseInfo licenseInfo);
    Task RequestManualReviewAsync(Guid evaluationId);
    Task ActivateProviderProfileAsync(Guid providerId);
}

public record MedicalBoardLicenseInfo(
    string LicenseNumber,
    string MedicalBoard,
    DateTime ExpiryDate,
    bool IsValid,
    string? Notes = null
);
