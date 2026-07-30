using Temporalio.Workflows;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Application.ProviderOnboarding;

[Workflow]
public interface IProviderOnboardingWorkflow
{
    [WorkflowRun]
    Task RunAsync(ProviderId providerId, LicenseNumber licenseNumber);
}
