using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.PlacementMatching.ValueObjects;

namespace TemporalDDD.Domain.ProviderCredentialing;

public sealed class ProviderProfile
{
    public ProviderProfileId Id { get; private set; }
    public ProviderPublicId? PublicId { get; private set; }
    public PersonName FirstName { get; private set; }
    public PersonName LastName { get; private set; }
    public Email Email { get; private set; }
    public Specialty Specialty { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset? ActivatedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public AggregateVersion Version { get; private set; }

    internal ProviderProfile() { }

    // Internal constructor for infrastructure rehydration
    internal ProviderProfile(ProviderProfileId id, ProviderPublicId? publicId, PersonName firstName, PersonName lastName, Email email, Specialty specialty, bool isActive, DateTimeOffset? activatedAt, DateTimeOffset createdAt, AggregateVersion version)
    {
        Id = id;
        PublicId = publicId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Specialty = specialty;
        IsActive = isActive;
        ActivatedAt = activatedAt;
        CreatedAt = createdAt;
        Version = version;
    }

    // Factory for creating new profile (ID is client-generated)
    public static ProviderProfile Create(PersonName firstName, PersonName lastName, Email email, Specialty specialty)
    {
        return new ProviderProfile
        {
            Id = ProviderProfileId.New(),
            PublicId = ProviderPublicId.New(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Specialty = specialty,
            IsActive = false,
            CreatedAt = DateTimeOffset.UtcNow,
            Version = AggregateVersion.Initial()
        };
    }

    

    public void Activate()
    {
        if (IsActive)
            throw new InvalidOperationException("Provider profile is already active");

        IsActive = true;
        ActivatedAt = DateTimeOffset.UtcNow;
        Version = Version.Increment();
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("Provider profile is not active");

        IsActive = false;
        ActivatedAt = null;
        Version = Version.Increment();
    }
}
