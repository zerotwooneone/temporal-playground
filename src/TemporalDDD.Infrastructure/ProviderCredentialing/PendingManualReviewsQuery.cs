using Microsoft.EntityFrameworkCore;
using TemporalDDD.Application.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.ProviderCredentialing;

public class PendingManualReviewsQuery : IPendingManualReviewsQuery
{
    private readonly ApplicationDbContext _dbContext;

    public PendingManualReviewsQuery(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<PendingManualReviewDto>> GetPendingReviewsAsync(CancellationToken cancellationToken = default)
    {
        var manualReviewRequiredStatus = EvaluationStatus.ManualReviewRequired.Value;

        var pendingReviews = await _dbContext.CredentialEvaluations
            .AsNoTracking()
            .Where(e => e.Status == manualReviewRequiredStatus && e.WorkflowId != null)
            .OrderBy(e => e.EvaluatedAt)
            .Select(e => new PendingManualReviewDto(
                EvaluationId: e.Id,
                ProviderId: e.ProviderId,
                LicenseNumber: e.LicenseNumber,
                MedicalBoard: e.MedicalBoard,
                EvaluatedAt: DateTimeOffset.FromUnixTimeMilliseconds(e.EvaluatedAt),
                WorkflowId: e.WorkflowId!
            ))
            .ToListAsync(cancellationToken);

        return pendingReviews;
    }
}
