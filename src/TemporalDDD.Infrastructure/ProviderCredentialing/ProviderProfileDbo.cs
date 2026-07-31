namespace TemporalDDD.Infrastructure.ProviderCredentialing;

public class ProviderProfileDbo
{
    public string Id { get; set; }
    public string? PublicId { get; set; }
    public string ProviderId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Specialty { get; set; }
    public bool IsActive { get; set; }
    public long? ActivatedAt { get; set; } // Unix milliseconds
    public long CreatedAt { get; set; } // Unix milliseconds
    public int Version { get; set; }
}
