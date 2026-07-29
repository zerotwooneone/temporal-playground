using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderCredentialing;

public class CredentialEvaluation
{
    public Guid Id { get; private set; }
    public ProviderId ProviderId { get; private set; }
    public LicenseNumber LicenseNumber { get; private set; }
    public MedicalBoard MedicalBoard { get; private set; }
    public LicenseExpiryDate LicenseExpiryDate { get; private set; }
    public bool IsCompliant { get; private set; }
    public string? ComplianceNotes { get; private set; }
    public DateTime EvaluatedAt { get; private set; }
    public EvaluationStatus Status { get; private set; }

    private CredentialEvaluation() { }

    public CredentialEvaluation(ProviderId providerId, LicenseNumber licenseNumber, MedicalBoard medicalBoard, LicenseExpiryDate licenseExpiryDate)
    {
        Id = Guid.NewGuid();
        ProviderId = providerId;
        LicenseNumber = licenseNumber;
        MedicalBoard = medicalBoard;
        LicenseExpiryDate = licenseExpiryDate;
        Status = EvaluationStatus.Pending;
        EvaluatedAt = DateTime.UtcNow;
    }

    public void MarkAsCompliant(string? notes = null)
    {
        IsCompliant = true;
        ComplianceNotes = notes;
        Status = EvaluationStatus.Approved;
    }

    public void MarkAsNonCompliant(string notes)
    {
        IsCompliant = false;
        ComplianceNotes = notes;
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
