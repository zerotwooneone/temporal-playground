using Microsoft.EntityFrameworkCore;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.ProviderCredentialing;

public class CredentialEvaluationRepository : ICredentialEvaluationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CredentialEvaluationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CredentialEvaluation?> GetByIdAsync(CredentialEvaluationId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.CredentialEvaluations
            .FirstOrDefaultAsync(e => e.Id == id.Value, cancellationToken);
    }

    public async Task SaveAsync(CredentialEvaluation aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.CredentialEvaluations
            .FirstOrDefaultAsync(e => e.Id == aggregate.Id.Value, cancellationToken);

        if (existing == null)
        {
            _dbContext.CredentialEvaluations.Add(aggregate);
        }
        else
        {
            _dbContext.Entry(existing).State = EntityState.Detached;
            _dbContext.CredentialEvaluations.Attach(aggregate);
            _dbContext.Entry(aggregate).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
