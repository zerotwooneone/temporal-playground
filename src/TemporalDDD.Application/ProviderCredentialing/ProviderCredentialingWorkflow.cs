using Temporalio.Workflows;
using TemporalDDD.Application.ProviderCredentialing;

namespace TemporalDDD.Application.ProviderCredentialing;

[Workflow]
public class ProviderCredentialingWorkflow
{
    private ManualReviewCompletedSignal? _manualReviewSignal;

    [WorkflowRun]
    public async Task RunAsync(Guid providerId, string licenseNumber, string medicalBoard, DateTime licenseExpiryDate)
    {
        // Step 1: Fetch Medical Board License (External API Read)
        var licenseInfo = await Workflow.ExecuteActivityAsync(
            (IProviderCredentialingActivities activities) => activities.FetchMedicalBoardLicenseAsync(licenseNumber, medicalBoard),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
        );

        // Step 2: Evaluate and Save Compliance (DB Write)
        var evaluationId = Workflow.NewGuid();
        await Workflow.ExecuteActivityAsync(
            (IProviderCredentialingActivities activities) => activities.EvaluateAndSaveComplianceAsync(evaluationId, licenseInfo),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
        );

        // Step 3: Request Manual Review if needed (External Notification)
        if (!licenseInfo.IsValid)
        {
            await Workflow.ExecuteActivityAsync(
                (IProviderCredentialingActivities activities) => activities.RequestManualReviewAsync(evaluationId),
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
            (IProviderCredentialingActivities activities) => activities.ActivateProviderProfileAsync(providerId),
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
