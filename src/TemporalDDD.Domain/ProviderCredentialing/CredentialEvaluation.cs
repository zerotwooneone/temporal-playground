namespace TemporalDDD.Domain.ProviderCredentialing;

public class CredentialEvaluation
{
    public Guid Id { get; private set; }
    public Guid ProviderId { get; private set; }
    public string LicenseNumber { get; private set; }
    public string MedicalBoard { get; private set; }
    public DateTime LicenseExpiryDate { get; private set; }
    public bool IsCompliant { get; private set; }
    public string? ComplianceNotes { get; private set; }
    public DateTime EvaluatedAt { get; private set; }
    public EvaluationStatus Status { get; private set; }

    private CredentialEvaluation() { }

    public CredentialEvaluation(Guid providerId, string licenseNumber, string medicalBoard, DateTime licenseExpiryDate)
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

public enum EvaluationStatus
{
    Pending,
    Approved,
    Rejected,
    ManualReviewRequired
}
