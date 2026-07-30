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
        // Elevate to Domain types with catastrophic assertions
        var providerIdResult = ProviderId.Create(input.ProviderId);
        if (providerIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid ProviderId. {providerIdResult.Error}");

        var licenseNumberResult = LicenseNumber.Create(input.LicenseNumber);
        if (licenseNumberResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid LicenseNumber. {licenseNumberResult.Error}");

        var providerId = providerIdResult.Value;
        var licenseNumber = licenseNumberResult.Value;
        EvaluationStatus status = await Workflow.ExecuteActivityAsync(
            (IComplianceActivities a) => a.PerformComplianceCheck(new PerformComplianceInput(licenseNumber.Value)),
            new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });

        if (status == EvaluationStatus.Approved)
        {
            await Workflow.ExecuteActivityAsync(
                (IProviderActivities a) => a.ActivateProvider(new ActivateProviderInput(providerId.Value, (int)status)),
                new() { StartToCloseTimeout = TimeSpan.FromMinutes(5) });
        }
    }
}
