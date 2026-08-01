using TemporalDDD.Application.Messaging;

namespace TemporalDDD.Application.ProviderCredentialing;

public sealed record CredentialEvaluationApprovedEvent(
    string EvaluationId,
    string? ComplianceNotes) : IApplicationEvent;
