using Temporalio.Activities;
using TemporalDDD.Domain.ProviderCredentialing;

namespace TemporalDDD.Application.ProviderOnboarding;

public interface IComplianceActivities
{
    [Activity]
    Task<EvaluationStatus> PerformComplianceCheck(string licenseNumber);
}
