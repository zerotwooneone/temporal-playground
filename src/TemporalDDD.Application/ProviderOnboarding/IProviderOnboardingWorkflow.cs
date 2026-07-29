using Temporalio.Workflows;
using TemporalDDD.Domain.ProviderOnboarding;

namespace TemporalDDD.Application.ProviderOnboarding;

[Workflow]
public interface IProviderOnboardingWorkflow
{
    [WorkflowRun]
    Task RunAsync(string providerId, string licenseNumber);
}
