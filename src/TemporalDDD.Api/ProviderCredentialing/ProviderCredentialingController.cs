using Microsoft.AspNetCore.Mvc;
using Temporalio.Client;
using TemporalDDD.Application.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Api.ProviderCredentialing;

[ApiController]
[Route("api/[controller]")]
public class ProviderCredentialingController : ControllerBase
{
    private readonly ITemporalClient _temporalClient;
    private readonly IPendingManualReviewsQuery _pendingReviewsQuery;

    public ProviderCredentialingController(ITemporalClient temporalClient, IPendingManualReviewsQuery pendingReviewsQuery)
    {
        _temporalClient = temporalClient;
        _pendingReviewsQuery = pendingReviewsQuery;
    }

    [HttpPost]
    public async Task<IActionResult> StartCredentialing([FromBody] StartCredentialingRequest request)
    {
        var workflowId = $"provider-credentialing-{Guid.NewGuid():N}";
        
        
        
        // Validate at the edge (Fail fast with HTTP 400 - No exceptions thrown)
        var licenseNumberResult = LicenseNumber.Create(request.LicenseNumber);
        if (licenseNumberResult.IsFailure) return BadRequest(licenseNumberResult.Error);

        var medicalBoardResult = MedicalBoard.Create(request.MedicalBoard);
        if (medicalBoardResult.IsFailure) return BadRequest(medicalBoardResult.Error);

        var expiryDateResult = LicenseExpiryDate.Create(request.ExpiryDate);
        if (expiryDateResult.IsFailure) return BadRequest(expiryDateResult.Error);
        
        // Generate ProviderId and ProviderPublicId server-side
        var providerId = ProviderId.New();
        var providerPublicId = ProviderPublicId.New();

        // Map the validated domain values into the Primitive DTO
        var workflowInput = new CredentialingInput(
            providerId.ToString(),
            providerPublicId.ToString(),
            licenseNumberResult.Value.Value,
            request.MedicalBoard,
            request.ExpiryDate,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Specialty
        );

        // Pass the single JSON-friendly object to Temporal
        await _temporalClient.StartWorkflowAsync(
            (ProviderCredentialingWorkflow wf) => wf.RunAsync(workflowInput),
            new WorkflowOptions
            {
                Id = workflowId,
                TaskQueue = "ONBOARDING_TASK_QUEUE",
                Memo = new Dictionary<string, object>
                {
                    ["ProviderId"] = providerId.ToString()
                }
            });

        return Ok(new { WorkflowId = workflowId, ProviderPublicId = providerPublicId.ToString(), Message = "Provider credentialing workflow started" });
    }

    [HttpGet("pending-reviews")]
    public async Task<IActionResult> GetPendingReviews()
    {
        var pendingReviews = await _pendingReviewsQuery.GetPendingReviewsAsync();
        return Ok(pendingReviews);
    }

    [HttpPost("{workflowId}/manual-review")]
    public async Task<IActionResult> CompleteManualReview(string workflowId, [FromBody] ManualReviewRequest request)
    {
        var handle = _temporalClient.GetWorkflowHandle(workflowId);
        await handle.SignalAsync(
            (ProviderCredentialingWorkflow wf) => wf.ManualReviewCompletedAsync(request.IsApproved, request.Notes));

        return Ok(new { Message = "Manual review signal sent" });
    }

    public record StartCredentialingRequest(string LicenseNumber, string MedicalBoard, DateTimeOffset ExpiryDate, string FirstName, string LastName, string Email, string Specialty);
    public record ManualReviewRequest(bool IsApproved, string? Notes);
}
