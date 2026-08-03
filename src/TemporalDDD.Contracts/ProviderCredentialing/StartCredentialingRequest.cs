namespace TemporalDDD.Contracts.ProviderCredentialing;

public sealed record StartCredentialingRequest(
    string LicenseNumber,
    string MedicalBoard,
    DateOnly ExpiryDate,
    string FirstName,
    string LastName,
    string Email,
    string Specialty);
