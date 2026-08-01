using TemporalDDD.Application.Messaging;

namespace TemporalDDD.Application.ProviderCredentialing;

public sealed record CredentialEvaluationRejectedEvent(
    string EvaluationId,
    string ComplianceNotes) : IApplicationEvent;
