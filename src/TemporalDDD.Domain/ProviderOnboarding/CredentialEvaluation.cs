using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderOnboarding;

public class CredentialEvaluation
{
    public uint Id { get; private set; }
    public CredentialEvaluationPublicId? PublicId { get; private set; }
    public LicenseNumber LicenseNumber { get; private set; }
    public ComplianceStatus Status { get; private set; }

    private CredentialEvaluation() { }

    // Factory for creating new evaluation (ID will be set by database)
    public static CredentialEvaluation Create(LicenseNumber licenseNumber)
    {
        return new CredentialEvaluation
        {
            Id = 0, // Temporary, will be set by DB
            PublicId = CredentialEvaluationPublicId.New(),
            LicenseNumber = licenseNumber,
            Status = ComplianceStatus.Pending
        };
    }

    // Factory for rehydrating from database
    public static CredentialEvaluation FromDatabase(uint id, Guid? publicId, LicenseNumber licenseNumber, ComplianceStatus status)
    {
        return new CredentialEvaluation
        {
            Id = id,
            PublicId = publicId.HasValue ? CredentialEvaluationPublicId.Create(publicId.Value) : null,
            LicenseNumber = licenseNumber,
            Status = status
        };
    }

    public ComplianceStatus EvaluateReport(bool isLicenseValid)
    {
        Status = isLicenseValid ? ComplianceStatus.Cleared : ComplianceStatus.Rejected;
        return Status;
    }
}
