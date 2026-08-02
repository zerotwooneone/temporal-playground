namespace TemporalDDD.Contracts.ProviderCredentialing;

public sealed record CredentialEvaluationRejectedEvent(
    string EvaluationPublicId,
    string ComplianceNotes);
