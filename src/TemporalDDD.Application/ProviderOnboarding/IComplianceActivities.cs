using Temporalio.Activities;
using TemporalDDD.Domain.ProviderOnboarding;

namespace TemporalDDD.Application.ProviderOnboarding;

public interface IComplianceActivities
{
    [Activity]
    Task<ComplianceStatus> PerformComplianceCheck(string licenseNumber);
}
