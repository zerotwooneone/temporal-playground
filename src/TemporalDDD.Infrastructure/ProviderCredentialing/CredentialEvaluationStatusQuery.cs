using Microsoft.EntityFrameworkCore;
using TemporalDDD.Application.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.ProviderCredentialing;

public class CredentialEvaluationStatusQuery : ICredentialEvaluationStatusQuery
{
    private readonly ApplicationDbContext _dbContext;

    public CredentialEvaluationStatusQuery(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CredentialEvaluationStatusDto?> GetByProviderIdAsync(string providerId, CancellationToken cancellationToken = default)
    {
        var evaluation = await _dbContext.CredentialEvaluations
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ProviderId == providerId, cancellationToken);

        if (evaluation == null)
            return null;

        var statusResult = EvaluationStatus.FromValue(evaluation.Status);
        if (statusResult.IsFailure)
            return null;

        var status = statusResult.Value;
        var (step, isWaitingForManualReview) = MapStatusToStep(status);

        return new CredentialEvaluationStatusDto(
            Status: status.Name,
            Step: step,
            IsWaitingForManualReview: isWaitingForManualReview,
            IsCompliant: evaluation.IsCompliant,
            Notes: evaluation.ComplianceNotes
        );
    }

    private (int Step, bool IsWaitingForManualReview) MapStatusToStep(EvaluationStatus status)
    {
        return status switch
        {
            var s when s == EvaluationStatus.Pending => (0, false),
            var s when s == EvaluationStatus.ManualReviewRequired => (2, true),
            var s when s == EvaluationStatus.Approved => (3, false),
            var s when s == EvaluationStatus.Rejected => (2, false),
            _ => (1, false)
        };
    }
}
