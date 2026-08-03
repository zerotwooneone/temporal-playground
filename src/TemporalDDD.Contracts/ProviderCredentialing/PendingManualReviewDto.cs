namespace TemporalDDD.Contracts.ProviderCredentialing;

public sealed record PendingManualReviewDto(
    string EvaluationPublicId,
    string ProviderPublicId,
    string LicenseNumber,
    string MedicalBoard,
    DateTimeOffset EvaluatedAt,
    string TrackingToken,
    string? ComplianceNotes);
