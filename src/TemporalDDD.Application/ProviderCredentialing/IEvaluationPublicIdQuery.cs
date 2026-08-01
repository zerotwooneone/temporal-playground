namespace TemporalDDD.Application.ProviderCredentialing;

public interface IEvaluationPublicIdQuery
{
    Task<string?> GetEvaluationPublicIdAsync(string evaluationId, CancellationToken cancellationToken = default);
}
