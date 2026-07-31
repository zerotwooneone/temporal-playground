namespace TemporalDDD.Application.ProviderCredentialing;

public interface ICredentialEvaluationStatusQuery
{
    Task<CredentialEvaluationStatusDto?> GetByProviderIdAsync(string providerId, CancellationToken cancellationToken = default);
}

public record CredentialEvaluationStatusDto(
    string Status,
    int Step,
    bool IsWaitingForManualReview,
    bool? IsCompliant,
    string? Notes
);
