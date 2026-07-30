namespace TemporalDDD.Application.ProviderCredentialing;

/// <summary>
/// Primitive DTO for Provider Credentialing Workflow input.
/// All fields are primitive types to ensure clean JSON serialization with Temporal.
/// </summary>
public record CredentialingInput(
    uint ProviderId,
    string LicenseNumber,
    string MedicalBoard,
    DateTime ExpiryDate
);
