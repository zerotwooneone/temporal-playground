namespace TemporalDDD.Application.ProviderCredentialing;

/// <summary>
/// Primitive DTO for Provider Credentialing Workflow input.
/// All fields are primitive types to ensure clean JSON serialization with Temporal.
/// </summary>
public record CredentialingInput(
    string ProviderId,
    string LicenseNumber,
    string MedicalBoard,
    DateTimeOffset ExpiryDate,
    string FirstName,
    string LastName,
    string Email,
    string Specialty
);
