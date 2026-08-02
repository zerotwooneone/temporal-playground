namespace TemporalDDD.Contracts.ProviderCredentialing;

public sealed record CredentialEvaluationApprovedEvent(
    string EvaluationPublicId,
    string? ComplianceNotes);
