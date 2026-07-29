using Temporalio.Workflows;
using TemporalDDD.Domain.ProviderCredentialing;

namespace TemporalDDD.Application.ProviderOnboarding;

public class ProviderOnboardingWorkflow : IProviderOnboardingWorkflow
{
    public async Task RunAsync(uint providerId, string licenseNumber)
    {
        EvaluationStatus status = await Workflow.ExecuteActivityAsync(
            (IComplianceActivities a) => a.PerformComplianceCheck(licenseNumber),
            new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

        if (status == EvaluationStatus.Approved)
        {
            await Workflow.ExecuteActivityAsync(
                (IProviderActivities a) => a.ActivateProvider(providerId, status),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });
        }
    }
}
