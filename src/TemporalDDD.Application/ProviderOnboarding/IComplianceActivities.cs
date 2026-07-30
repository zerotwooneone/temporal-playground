using Temporalio.Activities;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;

namespace TemporalDDD.Application.ProviderOnboarding;

public interface IComplianceActivities
{
    [Activity]
    Task<EvaluationStatus> PerformComplianceCheck(LicenseNumber licenseNumber);
}
