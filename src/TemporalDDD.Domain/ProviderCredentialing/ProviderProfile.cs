namespace TemporalDDD.Domain.ProviderCredentialing;

public class ProviderProfile
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string Specialty { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? ActivatedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public int Version { get; private set; }

    private ProviderProfile() { }

    public ProviderProfile(string firstName, string lastName, string email, string specialty)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Specialty = specialty;
        IsActive = false;
        CreatedAt = DateTime.UtcNow;
        Version = 1;
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
