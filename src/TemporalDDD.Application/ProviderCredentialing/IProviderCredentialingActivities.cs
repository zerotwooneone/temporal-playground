using TemporalDDD.Application.Messaging;
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
    Task<IReadOnlyCollection<IApplicationEvent>> RequestManualReviewAsync(RequestManualReviewInput input);
    [Activity]
    Task<string> GetOrCreateProviderProfileAsync(GetOrCreateProviderProfileInput input);
    [Activity]
    Task ActivateProviderProfileAsync(ActivateProviderProfileInput input);
    [Activity]
    Task<IReadOnlyCollection<IApplicationEvent>> UpdateEvaluationStatusAsync(UpdateEvaluationStatusInput input);
    [Activity]
    Task PublishApplicationEventsAsync(PublishApplicationEventsInput input);
}

// Primitive DTOs for activity parameters
public record FetchLicenseInput(string LicenseNumber, string MedicalBoard);
public record EvaluateComplianceInput(string ProviderId, string EvaluationPublicId, string LicenseNumber, string MedicalBoard, DateOnly ExpiryDate, bool IsValid, string? Notes = null);
public record RequestManualReviewInput(string EvaluationId, string WorkflowId);
public record GetOrCreateProviderProfileInput(string ProviderId, string ProviderPublicId, string FirstName, string LastName, string Email, string Specialty);
public record ActivateProviderProfileInput(string ProviderProfileId);
public record PublishApplicationEventsInput(IReadOnlyCollection<IApplicationEvent> Events);

// Primitive result types
public record MedicalBoardLicenseInfo(
    string LicenseNumber,
    string MedicalBoard,
    DateOnly ExpiryDate,
    bool IsValid,
    string? Notes = null
);

public record EvaluateComplianceResult(
    string EvaluationId,
    bool IsValid,
    bool IsCompliant,
    IReadOnlyCollection<IApplicationEvent> Events
);

public record UpdateEvaluationStatusInput(string EvaluationId, bool IsCompliant, string? Notes = null);
