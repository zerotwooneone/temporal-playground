using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;

namespace TemporalDDD.Domain.ProviderOnboarding;

public class CredentialEvaluation
{
    public LicenseNumber LicenseNumber { get; }
    public ComplianceStatus Status { get; private set; }

    public CredentialEvaluation(LicenseNumber licenseNumber)
    {
        LicenseNumber = licenseNumber;
        Status = ComplianceStatus.Pending;
    }

    public ComplianceStatus EvaluateReport(bool isLicenseValid)
    {
        Status = isLicenseValid ? ComplianceStatus.Cleared : ComplianceStatus.Rejected;
        return Status;
    }
}
