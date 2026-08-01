namespace TemporalDDD.Application.ProviderCredentialing;

/// <summary>
/// Primitive DTO for Provider Credentialing Workflow input.
/// All fields are primitive types to ensure clean JSON serialization with Temporal.
/// </summary>
public record CredentialingInput(
    string ProviderId,
    string ProviderPublicId,
    string EvaluationPublicId,
    string LicenseNumber,
    string MedicalBoard,
    DateOnly ExpiryDate,
    string FirstName,
    string LastName,
    string Email,
    string Specialty
);
