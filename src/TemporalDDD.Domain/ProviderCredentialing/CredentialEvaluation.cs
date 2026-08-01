using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SeedWork;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderCredentialing;

public sealed class CredentialEvaluation : AggregateRoot
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
    public string? WorkflowId { get; private set; }

    internal CredentialEvaluation() { }

    // Internal constructor for infrastructure rehydration
    internal CredentialEvaluation(CredentialEvaluationId id, CredentialEvaluationPublicId? publicId, ProviderId providerId, LicenseNumber licenseNumber, MedicalBoard medicalBoard, LicenseExpiryDate licenseExpiryDate, bool isCompliant, ComplianceNotes complianceNotes, DateTimeOffset evaluatedAt, EvaluationStatus status, string? workflowId = null)
    {
        Id = id;
        PublicId = publicId;
        ProviderId = providerId;
        LicenseNumber = licenseNumber;
        MedicalBoard = medicalBoard;
        LicenseExpiryDate = licenseExpiryDate;
        IsCompliant = isCompliant;
        ComplianceNotes = complianceNotes;
        EvaluatedAt = evaluatedAt;
        Status = status;
        WorkflowId = workflowId;
    }

    // Factory for creating new evaluation (ID is client-generated)
    public static CredentialEvaluation Create(ProviderId providerId, LicenseNumber licenseNumber, MedicalBoard medicalBoard, LicenseExpiryDate licenseExpiryDate)
    {
        var credentialEvaluation = new CredentialEvaluation
        {
            Id = CredentialEvaluationId.New(),
            PublicId = CredentialEvaluationPublicId.New(),
            ProviderId = providerId,
            LicenseNumber = licenseNumber,
            MedicalBoard = medicalBoard,
            LicenseExpiryDate = licenseExpiryDate,
            Status = EvaluationStatus.Pending,
            EvaluatedAt = DateTimeOffset.UtcNow,
            ComplianceNotes = ComplianceNotes.Create(null).Value!
        };
        credentialEvaluation.RaiseDomainEvent(new CredentialEvaluationCreated(CredentialEvaluationId.New(), providerId, EvaluationStatus.Pending));
        return credentialEvaluation;
    }

    public void MarkAsCompliant(string? notes = null)
    {
        IsCompliant = true;
        ComplianceNotes = ComplianceNotes.Create(notes).Value ?? throw new InvalidOperationException("Invalid ComplianceNotes");
        Status = EvaluationStatus.Approved;
        RaiseDomainEvent(new CredentialEvaluationApproved(Id, ComplianceNotes));
    }

    public void MarkAsNonCompliant(string notes)
    {
        IsCompliant = false;
        ComplianceNotes = ComplianceNotes.Create(notes).Value ?? throw new InvalidOperationException("Invalid ComplianceNotes");
        Status = EvaluationStatus.Rejected;
        RaiseDomainEvent(new CredentialEvaluationRejected(Id, ComplianceNotes));
    }

    public void RequestManualReview(string workflowId)
    {
        Status = EvaluationStatus.ManualReviewRequired;
        WorkflowId = workflowId;
        RaiseDomainEvent(new CredentialEvaluationRequiresManualReview(Id));
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

public sealed record CredentialEvaluationCreated(
    CredentialEvaluationId EvaluationId, 
    ProviderId ProviderId, 
    EvaluationStatus TargetStatus) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record CredentialEvaluationApproved(
    CredentialEvaluationId EvaluationId, 
    ComplianceNotes? ComplianceNotes) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record CredentialEvaluationRejected(
    CredentialEvaluationId EvaluationId, 
    ComplianceNotes ComplianceNotes) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTime.UtcNow;
}

public sealed record CredentialEvaluationRequiresManualReview(
    CredentialEvaluationId EvaluationId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTime.UtcNow;
}
