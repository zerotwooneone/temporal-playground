using TemporalDDD.Application.Messaging;

namespace TemporalDDD.Application.ProviderCredentialing;

public sealed record CredentialEvaluationCreatedEvent(
    string EvaluationId,
    string ProviderId,
    int TargetStatus) : IApplicationEvent;
