using Microsoft.EntityFrameworkCore;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.Queries.ProviderCredentialing;

public class PendingManualReviewsQuery : Application.ProviderCredentialing.IPendingManualReviewsQuery
{
    private readonly ApplicationDbContext _dbContext;

    public PendingManualReviewsQuery(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Contracts.ProviderCredentialing.PendingManualReviewDto>> GetPendingReviewsAsync(CancellationToken cancellationToken = default)
    {
        var manualReviewRequiredStatus = EvaluationStatus.ManualReviewRequired.Value;

        var pendingReviews = await _dbContext.CredentialEvaluations
            .AsNoTracking()
            .Join(
                _dbContext.ProviderProfiles,
                evaluation => evaluation.ProviderId,
                profile => profile.ProviderId,
                (evaluation, profile) => new { evaluation, profile })
            .Where(x => x.evaluation.Status == manualReviewRequiredStatus && x.evaluation.WorkflowId != null)
            .OrderBy(x => x.evaluation.EvaluatedAt)
            .Select(x => new Contracts.ProviderCredentialing.PendingManualReviewDto(
                EvaluationPublicId: x.evaluation.PublicId,
                ProviderPublicId: x.profile.PublicId,
                LicenseNumber: x.evaluation.LicenseNumber,
                MedicalBoard: x.evaluation.MedicalBoard,
                EvaluatedAt: DateTimeOffset.FromUnixTimeMilliseconds(x.evaluation.EvaluatedAt),
                TrackingToken: x.evaluation.WorkflowId!
            ))
            .ToListAsync(cancellationToken);

        return pendingReviews;
    }
}
