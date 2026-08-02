namespace TemporalDDD.Contracts.ProviderCredentialing;

public sealed record CredentialEvaluationCreatedEvent(
    string EvaluationPublicId,
    string ProviderId,
    int TargetStatus);
