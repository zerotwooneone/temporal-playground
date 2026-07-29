using Temporalio.Workflows;
using TemporalDDD.Domain.ProviderCredentialing;

namespace TemporalDDD.Application.ProviderOnboarding;

[Workflow]
public interface IProviderOnboardingWorkflow
{
    [WorkflowRun]
    Task RunAsync(uint providerId, string licenseNumber);
}
