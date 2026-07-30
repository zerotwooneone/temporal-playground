using Microsoft.EntityFrameworkCore;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.ProviderCredentialing;

public class ProviderProfileRepository : IProviderProfileRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ProviderProfileRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProviderProfile?> GetByIdAsync(ProviderProfileId id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProviderProfiles
            .FirstOrDefaultAsync(p => p.Id == id.Value, cancellationToken);
    }

    public async Task SaveAsync(ProviderProfile aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.ProviderProfiles
            .FirstOrDefaultAsync(p => p.Id == aggregate.Id.Value, cancellationToken);

        if (existing == null)
        {
            _dbContext.ProviderProfiles.Add(aggregate);
        }
        else
        {
            _dbContext.Entry(existing).State = EntityState.Detached;
            _dbContext.ProviderProfiles.Attach(aggregate);
            _dbContext.Entry(aggregate).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
