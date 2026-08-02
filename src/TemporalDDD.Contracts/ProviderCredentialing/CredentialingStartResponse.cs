namespace TemporalDDD.Contracts.ProviderCredentialing;

public sealed record CredentialingStartResponse(
    string TrackingToken,
    string ProviderPublicId,
    string EvaluationPublicId,
    string Message);
