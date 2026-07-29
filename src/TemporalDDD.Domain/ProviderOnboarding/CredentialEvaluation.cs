namespace TemporalDDD.Domain.ProviderOnboarding;

public class CredentialEvaluation
{
    public string LicenseNumber { get; }
    public ComplianceStatus Status { get; private set; }

    public CredentialEvaluation(string licenseNumber)
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
