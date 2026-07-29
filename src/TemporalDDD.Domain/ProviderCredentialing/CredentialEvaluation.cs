using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderCredentialing;

public class CredentialEvaluation
{
    public CredentialEvaluationId Id { get; private set; }
    public CredentialEvaluationPublicId? PublicId { get; private set; }
    public ProviderId ProviderId { get; private set; }
    public LicenseNumber LicenseNumber { get; private set; }
    public MedicalBoard MedicalBoard { get; private set; }
    public LicenseExpiryDate LicenseExpiryDate { get; private set; }
    public bool IsCompliant { get; private set; }
    public ComplianceNotes ComplianceNotes { get; private set; }
    public DateTimeOffset EvaluatedAt { get; private set; }
    public EvaluationStatus Status { get; private set; }

    private CredentialEvaluation() { }

    // Factory for creating new evaluation (ID will be set by database)
    public static CredentialEvaluation Create(ProviderId providerId, LicenseNumber licenseNumber, MedicalBoard medicalBoard, LicenseExpiryDate licenseExpiryDate)
    {
        return new CredentialEvaluation
        {
            Id = CredentialEvaluationId.Create(0), // Temporary, will be set by DB
            PublicId = CredentialEvaluationPublicId.New(),
            ProviderId = providerId,
            LicenseNumber = licenseNumber,
            MedicalBoard = medicalBoard,
            LicenseExpiryDate = licenseExpiryDate,
            Status = EvaluationStatus.Pending,
            EvaluatedAt = DateTimeOffset.UtcNow,
            ComplianceNotes = ComplianceNotes.Create(null)
        };
    }

    // Factory for rehydrating from database
    public static CredentialEvaluation FromDatabase(uint id, Guid? publicId, ProviderId providerId, LicenseNumber licenseNumber, MedicalBoard medicalBoard, LicenseExpiryDate licenseExpiryDate, bool isCompliant, string complianceNotes, DateTimeOffset evaluatedAt, EvaluationStatus status)
    {
        return new CredentialEvaluation
        {
            Id = CredentialEvaluationId.FromDatabase(id),
            PublicId = publicId.HasValue ? CredentialEvaluationPublicId.Create(publicId.Value) : null,
            ProviderId = providerId,
            LicenseNumber = licenseNumber,
            MedicalBoard = medicalBoard,
            LicenseExpiryDate = licenseExpiryDate,
            IsCompliant = isCompliant,
            ComplianceNotes = ComplianceNotes.Create(complianceNotes),
            EvaluatedAt = evaluatedAt,
            Status = status
        };
    }

    public void MarkAsCompliant(string? notes = null)
    {
        IsCompliant = true;
        ComplianceNotes = ComplianceNotes.Create(notes);
        Status = EvaluationStatus.Approved;
    }

    public void MarkAsNonCompliant(string notes)
    {
        IsCompliant = false;
        ComplianceNotes = ComplianceNotes.Create(notes);
        Status = EvaluationStatus.Rejected;
    }

    public void RequestManualReview()
    {
        Status = EvaluationStatus.ManualReviewRequired;
    }

    public void CompleteManualReview(bool approved, string? notes = null)
    {
        if (approved)
        {
            MarkAsCompliant(notes);
        }
        else
        {
            MarkAsNonCompliant(notes ?? "Manual review rejected");
        }
    }
}
