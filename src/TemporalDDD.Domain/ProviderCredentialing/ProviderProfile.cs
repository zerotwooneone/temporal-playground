using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderCredentialing;

public class ProviderProfile
{
    public uint Id { get; private set; }
    public ProviderPublicId? PublicId { get; private set; }
    public PersonName FirstName { get; private set; }
    public PersonName LastName { get; private set; }
    public Email Email { get; private set; }
    public Specialty Specialty { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? ActivatedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int Version { get; private set; }

    private ProviderProfile() { }

    // Factory for creating new profile (ID will be set by database)
    public static ProviderProfile Create(PersonName firstName, PersonName lastName, Email email, Specialty specialty)
    {
        return new ProviderProfile
        {
            Id = 0, // Temporary, will be set by DB
            PublicId = ProviderPublicId.New(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Specialty = specialty,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            Version = 1
        };
    }

    // Factory for rehydrating from database
    public static ProviderProfile FromDatabase(uint id, Guid? publicId, PersonName firstName, PersonName lastName, Email email, Specialty specialty, bool isActive, DateTime? activatedAt, DateTime createdAt, int version)
    {
        return new ProviderProfile
        {
            Id = id,
            PublicId = publicId.HasValue ? ProviderPublicId.Create(publicId.Value) : null,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Specialty = specialty,
            IsActive = isActive,
            ActivatedAt = activatedAt,
            CreatedAt = createdAt,
            Version = version
        };
    }

    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("Provider profile is already active");

        IsActive = true;
        ActivatedAt = DateTime.UtcNow;
        Version++;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("Provider profile is not active");

        IsActive = false;
        ActivatedAt = null;
        Version++;
    }
}
