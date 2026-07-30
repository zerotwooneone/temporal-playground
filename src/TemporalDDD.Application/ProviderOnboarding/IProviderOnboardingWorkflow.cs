using Temporalio.Workflows;

namespace TemporalDDD.Application.ProviderOnboarding;

[Workflow]
public interface IProviderOnboardingWorkflow
{
    [WorkflowRun]
    Task RunAsync(OnboardingInput input);
}
