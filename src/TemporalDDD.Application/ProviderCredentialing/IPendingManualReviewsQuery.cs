using TemporalDDD.Contracts.ProviderCredentialing;

namespace TemporalDDD.Application.ProviderCredentialing;

public interface IPendingManualReviewsQuery
{
    Task<List<PendingManualReviewDto>> GetPendingReviewsAsync(CancellationToken cancellationToken = default);
    Task<PendingManualReviewDto?> GetPendingReviewByEvaluationPublicIdAsync(string evaluationPublicId, CancellationToken cancellationToken = default);
}
