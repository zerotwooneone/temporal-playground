using Microsoft.EntityFrameworkCore;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.PlacementMatching.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
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
        var dbo = await _dbContext.ProviderProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id.ToString(), cancellationToken);

        if (dbo == null) return null;

        return MapToDomain(dbo);
    }

    public async Task<ProviderProfile?> GetByProviderIdAsync(ProviderId providerId, CancellationToken cancellationToken = default)
    {
        var dbo = await _dbContext.ProviderProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProviderId == providerId.ToString(), cancellationToken);

        if (dbo == null) return null;

        return MapToDomain(dbo);
    }

    public async Task SaveAsync(ProviderProfile aggregate, CancellationToken cancellationToken = default)
    {
        var idString = aggregate.Id.ToString();
        var existing = await _dbContext.ProviderProfiles
            .FirstOrDefaultAsync(p => p.Id == idString, cancellationToken);

        if (existing == null)
        {
            existing = new ProviderProfileDbo();
            MapToDbo(aggregate, existing);
            _dbContext.ProviderProfiles.Add(existing);
        }
        else
        {
            MapToDbo(aggregate, existing, existing.PublicId);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private ProviderProfile MapToDomain(ProviderProfileDbo dbo)
    {
        var firstName = PersonName.Create(dbo.FirstName).Value ?? throw new InvalidOperationException($"Invalid first name in database: {dbo.FirstName}");
        var lastName = PersonName.Create(dbo.LastName).Value ?? throw new InvalidOperationException($"Invalid last name in database: {dbo.LastName}");
        var email = Email.Create(dbo.Email).Value ?? throw new InvalidOperationException($"Invalid email in database: {dbo.Email}");
        var specialty = Specialty.Create(dbo.Specialty).Value ?? throw new InvalidOperationException($"Invalid specialty in database: {dbo.Specialty}");
        var version = AggregateVersion.Create(dbo.Version).Value ?? throw new InvalidOperationException($"Invalid version in database: {dbo.Version}");
        var createdAt = DateTimeOffset.FromUnixTimeMilliseconds(dbo.CreatedAt);
        var activatedAt = dbo.ActivatedAt.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(dbo.ActivatedAt.Value) : (DateTimeOffset?)null;

        var publicId = ProviderPublicId.Create(dbo.PublicId).Value ?? throw new InvalidOperationException($"Invalid public ID in database: {dbo.PublicId}");
        
        var id = ProviderProfileId.Create(dbo.Id).Value ?? throw new InvalidOperationException($"Invalid provider profile ID in database: {dbo.Id}");
        var providerId = ProviderId.Create(dbo.ProviderId).Value ?? throw new InvalidOperationException($"Invalid provider ID in database: {dbo.ProviderId}");

        // Use internal constructor for rehydration (infrastructure concern)
        return new ProviderProfile(
            id: id,
            publicId: publicId,
            providerId: providerId,
            firstName: firstName,
            lastName: lastName,
            email: email,
            specialty: specialty,
            isActive: dbo.IsActive,
            activatedAt: activatedAt,
            createdAt: createdAt,
            version: version
        );
    }

    private void MapToDbo(ProviderProfile profile, ProviderProfileDbo dbo, string? existingPublicId = null)
    {
        dbo.Id = profile.Id.ToString();
        dbo.PublicId = existingPublicId ?? profile.PublicId?.ToString();
        dbo.ProviderId = profile.ProviderId.ToString();
        dbo.FirstName = profile.FirstName.Value;
        dbo.LastName = profile.LastName.Value;
        dbo.Email = profile.Email.Value;
        dbo.Specialty = profile.Specialty.Value;
        dbo.IsActive = profile.IsActive;
        dbo.ActivatedAt = profile.ActivatedAt?.ToUnixTimeMilliseconds();
        dbo.CreatedAt = profile.CreatedAt.ToUnixTimeMilliseconds();
        dbo.Version = profile.Version.Value;
    }
}
