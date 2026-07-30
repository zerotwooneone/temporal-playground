using Temporalio.Workflows;
using TemporalDDD.Application.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Application.ProviderCredentialing;

[Workflow]
public class ProviderCredentialingWorkflow
{
    private ManualReviewCompletedSignal? _manualReviewSignal;

    [WorkflowRun]
    public async Task RunAsync(CredentialingInput input)
    {
        // Pass-through: No domain conversion needed - just pass primitives to activities
        // Step 1: Fetch Medical Board License (External API Read)
        var licenseInfo = await Workflow.ExecuteActivityAsync(
            (IProviderCredentialingActivities activities) => activities.FetchMedicalBoardLicenseAsync(new FetchLicenseInput(input.LicenseNumber, input.MedicalBoard)),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
        );

        // Step 2: Evaluate and Save Compliance (DB Write)
        var evaluationId = await Workflow.ExecuteActivityAsync(
            (IProviderCredentialingActivities activities) => activities.EvaluateAndSaveComplianceAsync(new EvaluateComplianceInput(
                input.ProviderId,
                licenseInfo.LicenseNumber,
                licenseInfo.MedicalBoard,
                licenseInfo.ExpiryDate,
                licenseInfo.IsValid,
                licenseInfo.ProviderId,
                licenseInfo.Notes
            )),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
        );

        // Step 3: Request Manual Review if needed (External Notification)
        if (!licenseInfo.IsValid)
        {
            await Workflow.ExecuteActivityAsync(
                (IProviderCredentialingActivities activities) => activities.RequestManualReviewAsync(new RequestManualReviewInput(evaluationId.Value)),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );

            // Step 4: Wait for Manual Review Completed Signal
            await Workflow.WaitConditionAsync(() => _manualReviewSignal is not null);

            if (_manualReviewSignal == null || !_manualReviewSignal.Approved)
            {
                throw new ApplicationFailedException("Manual review rejected or not completed");
            }
        }

        // Step 5: Activate Provider Profile (DB Write)
        await Workflow.ExecuteActivityAsync(
            (IProviderCredentialingActivities activities) => activities.ActivateProviderProfileAsync(new ActivateProviderProfileInput(input.ProviderId)),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
        );
    }

    [WorkflowSignal]
    public async Task ManualReviewCompletedAsync(bool approved, string? notes = null)
    {
        _manualReviewSignal = new ManualReviewCompletedSignal(approved, notes);
    }
}

public record ManualReviewCompletedSignal(bool Approved, string? Notes = null);

public class ApplicationFailedException : Exception
{
    public ApplicationFailedException(string message) : base(message) { }
}
