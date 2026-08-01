using TemporalDDD.Application.Messaging;

namespace TemporalDDD.Application.ProviderCredentialing;

public sealed record CredentialEvaluationRequiresManualReviewEvent(
    string EvaluationId) : IApplicationEvent;
