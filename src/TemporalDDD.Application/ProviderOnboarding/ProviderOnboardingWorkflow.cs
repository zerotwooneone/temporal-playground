using Temporalio.Workflows;
using TemporalDDD.Domain.ProviderOnboarding;

namespace TemporalDDD.Application.ProviderOnboarding;

public class ProviderOnboardingWorkflow : IProviderOnboardingWorkflow
{
    public async Task RunAsync(uint providerId, string licenseNumber)
    {
        ComplianceStatus status = await Workflow.ExecuteActivityAsync(
            (IComplianceActivities a) => a.PerformComplianceCheck(licenseNumber),
            new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

        if (status == ComplianceStatus.Cleared)
        {
            await Workflow.ExecuteActivityAsync(
                (IProviderActivities a) => a.ActivateProvider(providerId, status),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });
        }
    }
}
