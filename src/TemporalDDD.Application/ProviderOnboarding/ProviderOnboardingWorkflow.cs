using Temporalio.Workflows;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Application.ProviderOnboarding;

[Workflow]
public class ProviderOnboardingWorkflow : IProviderOnboardingWorkflow
{
    [WorkflowRun]
    public async Task RunAsync(OnboardingInput input)
    {
        // Pass-through: No domain conversion needed - just pass primitives to activities
        EvaluationStatus status = await Workflow.ExecuteActivityAsync(
            (IComplianceActivities a) => a.PerformComplianceCheck(new PerformComplianceInput(input.LicenseNumber)),
            new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

        if (status == EvaluationStatus.Approved)
        {
            await Workflow.ExecuteActivityAsync(
                (IProviderActivities a) => a.ActivateProvider(new ActivateProviderInput(input.ProviderId, (int)status)),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });
        }
    }
}
