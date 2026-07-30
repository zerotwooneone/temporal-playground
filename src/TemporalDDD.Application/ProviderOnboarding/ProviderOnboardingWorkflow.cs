using Temporalio.Workflows;
using TemporalDDD.Domain.ProviderCredentialing;

namespace TemporalDDD.Application.ProviderOnboarding;

[Workflow]
public class ProviderOnboardingWorkflow : IProviderOnboardingWorkflow
{
    [WorkflowRun]
    public async Task RunAsync(OnboardingInput input)
    {
        // Pass-through: No domain conversion needed - just pass primitives to activities
        int statusValue = await Workflow.ExecuteActivityAsync(
            (IComplianceActivities a) => a.PerformComplianceCheck(new PerformComplianceInput(input.LicenseNumber)),
            new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

        var statusResult = EvaluationStatus.FromValue(statusValue);
        if (statusResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid EvaluationStatus from activity. {statusResult.Error}");
        var status = statusResult.Value;

        if (status == EvaluationStatus.Approved)
        {
            await Workflow.ExecuteActivityAsync(
                (IProviderActivities a) => a.ActivateProvider(new ActivateProviderInput(input.ProviderId, statusValue)),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });
        }
    }
}
