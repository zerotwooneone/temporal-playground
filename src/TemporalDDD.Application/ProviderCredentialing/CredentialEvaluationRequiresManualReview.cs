using TemporalDDD.Application.Messaging;

namespace TemporalDDD.Application.ProviderCredentialing;

public sealed record CredentialEvaluationRequiresManualReview(
    string EvaluationId) : IApplicationEvent;
