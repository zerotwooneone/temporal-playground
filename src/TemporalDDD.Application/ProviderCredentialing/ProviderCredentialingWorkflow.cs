using Temporalio.Workflows;

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
        var evaluationResult = await Workflow.ExecuteActivityAsync(
            (IProviderCredentialingActivities activities) => activities.EvaluateAndSaveComplianceAsync(new EvaluateComplianceInput(
                input.ProviderId,
                input.EvaluationPublicId,
                licenseInfo.LicenseNumber,
                licenseInfo.MedicalBoard,
                licenseInfo.ExpiryDate,
                licenseInfo.IsValid,
                licenseInfo.ProviderId,
                licenseInfo.Notes
            )),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
        );

        // Publish application events from evaluation
        if (evaluationResult.Events.Count > 0)
        {
            await Workflow.ExecuteActivityAsync(
                (IProviderCredentialingActivities activities) => activities.PublishApplicationEventsAsync(new PublishApplicationEventsInput(evaluationResult.Events)),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );
        }

        // Step 3: Workflow orchestration decision based on evaluation result
        if (!licenseInfo.IsValid)
        {
            // Request manual review with workflow ID for correlation
            var manualReviewEvents = await Workflow.ExecuteActivityAsync(
                (IProviderCredentialingActivities activities) => activities.RequestManualReviewAsync(new RequestManualReviewInput(evaluationResult.EvaluationId, Workflow.Info.WorkflowId)),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );

            // Publish application events from manual review request
            if (manualReviewEvents.Count > 0)
            {
                await Workflow.ExecuteActivityAsync(
                    (IProviderCredentialingActivities activities) => activities.PublishApplicationEventsAsync(new PublishApplicationEventsInput(manualReviewEvents)),
                    new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
                );
            }

            // Wait for Manual Review Completed Signal
            await Workflow.WaitConditionAsync(() => _manualReviewSignal is not null);

            if (_manualReviewSignal == null || !_manualReviewSignal.Approved)
            {
                // Update evaluation status to rejected
                var rejectionNotes = _manualReviewSignal?.Notes ?? "Manual review rejected";
                var rejectionEvents = await Workflow.ExecuteActivityAsync(
                    (IProviderCredentialingActivities activities) => activities.UpdateEvaluationStatusAsync(new UpdateEvaluationStatusInput(
                        evaluationResult.EvaluationId,
                        IsCompliant: false,
                        Notes: rejectionNotes
                    )),
                    new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
                );

                // Publish application events from rejection
                if (rejectionEvents.Count > 0)
                {
                    await Workflow.ExecuteActivityAsync(
                        (IProviderCredentialingActivities activities) => activities.PublishApplicationEventsAsync(new PublishApplicationEventsInput(rejectionEvents)),
                        new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
                    );
                }

                throw new ApplicationFailedException("Manual review rejected or not completed");
            }
            else
            {
                // Update evaluation status to approved
                var approvalNotes = _manualReviewSignal?.Notes;
                var approvalEvents = await Workflow.ExecuteActivityAsync(
                    (IProviderCredentialingActivities activities) => activities.UpdateEvaluationStatusAsync(new UpdateEvaluationStatusInput(
                        evaluationResult.EvaluationId,
                        IsCompliant: true,
                        Notes: approvalNotes
                    )),
                    new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
                );

                // Publish application events from approval
                if (approvalEvents.Count > 0)
                {
                    await Workflow.ExecuteActivityAsync(
                        (IProviderCredentialingActivities activities) => activities.PublishApplicationEventsAsync(new PublishApplicationEventsInput(approvalEvents)),
                        new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
                    );
                }
            }
        }
        else
        {
            // License is valid - mark as compliant
            var approvalEvents = await Workflow.ExecuteActivityAsync(
                (IProviderCredentialingActivities activities) => activities.UpdateEvaluationStatusAsync(new UpdateEvaluationStatusInput(
                    evaluationResult.EvaluationId,
                    IsCompliant: true,
                    Notes: "License verified successfully"
                )),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
            );

            // Publish application events from approval
            if (approvalEvents.Count > 0)
            {
                await Workflow.ExecuteActivityAsync(
                    (IProviderCredentialingActivities activities) => activities.PublishApplicationEventsAsync(new PublishApplicationEventsInput(approvalEvents)),
                    new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
                );
            }
        }

        // Step 4: Get or Create Provider Profile (DB Write)
        var providerProfileId = await Workflow.ExecuteActivityAsync(
            (IProviderCredentialingActivities activities) => activities.GetOrCreateProviderProfileAsync(new GetOrCreateProviderProfileInput(
                input.ProviderId,
                input.ProviderPublicId,
                input.FirstName,
                input.LastName,
                input.Email,
                input.Specialty
            )),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
        );

        // Step 5: Activate Provider Profile (DB Write)
        await Workflow.ExecuteActivityAsync(
            (IProviderCredentialingActivities activities) => activities.ActivateProviderProfileAsync(new ActivateProviderProfileInput(providerProfileId)),
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
