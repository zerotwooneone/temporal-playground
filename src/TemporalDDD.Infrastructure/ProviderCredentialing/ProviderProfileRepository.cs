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
            .FirstOrDefaultAsync(p => p.Id == id.Value, cancellationToken);
        
        if (dbo == null) return null;
        
        return MapToDomain(dbo);
    }

    public async Task SaveAsync(ProviderProfile aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.ProviderProfiles
            .FirstOrDefaultAsync(p => p.Id == aggregate.Id.Value, cancellationToken);

        var dbo = MapToDbo(aggregate);

        if (existing == null)
        {
            _dbContext.ProviderProfiles.Add(dbo);
        }
        else
        {
            _dbContext.Entry(existing).State = EntityState.Detached;
            _dbContext.ProviderProfiles.Attach(dbo);
            _dbContext.Entry(dbo).State = EntityState.Modified;
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

        ProviderPublicId? publicId = null;
        if (!string.IsNullOrEmpty(dbo.PublicId))
        {
            publicId = ProviderPublicId.FromString(dbo.PublicId);
        }

        // Use reflection to call private constructor for rehydration
        var profile = (ProviderProfile)Activator.CreateInstance(
            typeof(ProviderProfile),
            nonPublic: true)!;
        
        // Set properties via reflection (infrastructure concern)
        typeof(ProviderProfile).GetProperty(nameof(ProviderProfile.Id))?.SetValue(profile, ProviderProfileId.Create(dbo.Id).Value ?? throw new InvalidOperationException($"Invalid provider profile ID in database: {dbo.Id}"));
        typeof(ProviderProfile).GetProperty(nameof(ProviderProfile.PublicId))?.SetValue(profile, publicId);
        typeof(ProviderProfile).GetProperty(nameof(ProviderProfile.FirstName))?.SetValue(profile, firstName);
        typeof(ProviderProfile).GetProperty(nameof(ProviderProfile.LastName))?.SetValue(profile, lastName);
        typeof(ProviderProfile).GetProperty(nameof(ProviderProfile.Email))?.SetValue(profile, email);
        typeof(ProviderProfile).GetProperty(nameof(ProviderProfile.Specialty))?.SetValue(profile, specialty);
        typeof(ProviderProfile).GetProperty(nameof(ProviderProfile.IsActive))?.SetValue(profile, dbo.IsActive);
        typeof(ProviderProfile).GetProperty(nameof(ProviderProfile.ActivatedAt))?.SetValue(profile, activatedAt);
        typeof(ProviderProfile).GetProperty(nameof(ProviderProfile.CreatedAt))?.SetValue(profile, createdAt);
        typeof(ProviderProfile).GetProperty(nameof(ProviderProfile.Version))?.SetValue(profile, version);

        return profile;
    }

    private ProviderProfileDbo MapToDbo(ProviderProfile profile)
    {
        return new ProviderProfileDbo
        {
            Id = profile.Id.Value,
            PublicId = profile.PublicId?.ToString(),
            FirstName = profile.FirstName.Value,
            LastName = profile.LastName.Value,
            Email = profile.Email.Value,
            Specialty = profile.Specialty.Value,
            IsActive = profile.IsActive,
            ActivatedAt = profile.ActivatedAt?.ToUnixTimeMilliseconds(),
            CreatedAt = profile.CreatedAt.ToUnixTimeMilliseconds(),
            Version = profile.Version.Value
        };
    }
}
