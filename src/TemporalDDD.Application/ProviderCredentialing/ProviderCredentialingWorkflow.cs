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
        // Elevate to Domain types with catastrophic assertions
        var providerIdResult = ProviderId.Create(input.ProviderId);
        if (providerIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid ProviderId. {providerIdResult.Error}");

        var licenseNumberResult = LicenseNumber.Create(input.LicenseNumber);
        if (licenseNumberResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid LicenseNumber. {licenseNumberResult.Error}");

        var medicalBoardResult = MedicalBoard.Create(input.MedicalBoard);
        if (medicalBoardResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid MedicalBoard. {medicalBoardResult.Error}");

        var licenseExpiryDateResult = LicenseExpiryDate.Create(input.ExpiryDate);
        if (licenseExpiryDateResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid LicenseExpiryDate. {licenseExpiryDateResult.Error}");

        var providerId = providerIdResult.Value;
        var licenseNumber = licenseNumberResult.Value;
        var medicalBoard = medicalBoardResult.Value;
        var licenseExpiryDate = licenseExpiryDateResult.Value;
        // Step 1: Fetch Medical Board License (External API Read)
        var licenseInfo = await Workflow.ExecuteActivityAsync(
            (IProviderCredentialingActivities activities) => activities.FetchMedicalBoardLicenseAsync(licenseNumber, medicalBoard),
            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) }
        );

        // Step 2: Evaluate and Save Compliance (DB Write)
        var evaluationId = await Workflow.ExecuteActivityAsync(
            (IProviderCredentialingActivities activities) => activities.EvaluateAndSaveComplianceAsync(providerId, licenseInfo),
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
        var providerProfileId = ProviderProfileId.Create(providerId.Value).Value!;
        await Workflow.ExecuteActivityAsync(
            (IProviderCredentialingActivities activities) => activities.ActivateProviderProfileAsync(providerProfileId),
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
