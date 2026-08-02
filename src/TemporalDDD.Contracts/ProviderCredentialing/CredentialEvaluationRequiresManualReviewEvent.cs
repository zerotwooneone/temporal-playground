namespace TemporalDDD.Contracts.ProviderCredentialing;

public sealed record CredentialEvaluationRequiresManualReviewEvent(
    string EvaluationPublicId);
