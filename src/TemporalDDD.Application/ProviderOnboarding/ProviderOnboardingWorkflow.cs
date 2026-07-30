using Temporalio.Workflows;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Application.ProviderOnboarding;

[Workflow]
public class ProviderOnboardingWorkflow : IProviderOnboardingWorkflow
{
    [WorkflowRun]
    public async Task RunAsync(ProviderId providerId, LicenseNumber licenseNumber)
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
