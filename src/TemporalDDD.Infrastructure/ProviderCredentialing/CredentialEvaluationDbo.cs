namespace TemporalDDD.Infrastructure.ProviderCredentialing;

public class CredentialEvaluationDbo
{
    public string Id { get; set; }
    public string? PublicId { get; set; }
    public string ProviderId { get; set; }
    public string LicenseNumber { get; set; }
    public string MedicalBoard { get; set; }
    public DateTimeOffset LicenseExpiryDate { get; set; }
    public bool IsCompliant { get; set; }
    public string? ComplianceNotes { get; set; }
    public long EvaluatedAt { get; set; } // Unix milliseconds
    public int Status { get; set; }
}
