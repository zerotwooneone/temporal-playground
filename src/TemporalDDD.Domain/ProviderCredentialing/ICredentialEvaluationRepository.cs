namespace TemporalDDD.Domain.ProviderCredentialing;

public interface ICredentialEvaluationRepository
{
    Task<CredentialEvaluation?> GetByIdAsync(CredentialEvaluationId id, CancellationToken cancellationToken = default);
    Task SaveAsync(CredentialEvaluation aggregate, CancellationToken cancellationToken = default);
}
