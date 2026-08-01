using Microsoft.EntityFrameworkCore;
using TemporalDDD.Application.ProviderCredentialing;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.ProviderCredentialing;

public class EvaluationPublicIdQuery : IEvaluationPublicIdQuery
{
    private readonly ApplicationDbContext _dbContext;

    public EvaluationPublicIdQuery(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string?> GetEvaluationPublicIdAsync(string evaluationId, CancellationToken cancellationToken = default)
    {
        var evaluation = await _dbContext.CredentialEvaluations
            .AsNoTracking()
            .Where(e => e.Id == evaluationId)
            .Select(e => e.PublicId)
            .FirstOrDefaultAsync(cancellationToken);

        return evaluation;
    }
}
