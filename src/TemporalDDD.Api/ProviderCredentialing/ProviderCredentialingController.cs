using Microsoft.AspNetCore.Mvc;
using Temporalio.Client;
using TemporalDDD.Application.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Api.ProviderCredentialing;

[ApiController]
[Route("api/[controller]")]
public class ProviderCredentialingController : ControllerBase
{
    private readonly ITemporalClient _temporalClient;
    private readonly ICredentialEvaluationStatusQuery _statusQuery;

    public ProviderCredentialingController(ITemporalClient temporalClient, ICredentialEvaluationStatusQuery statusQuery)
    {
        _temporalClient = temporalClient;
        _statusQuery = statusQuery;
    }

    [HttpPost]
    public async Task<IActionResult> StartCredentialing([FromBody] StartCredentialingRequest request)
    {
        var workflowId = $"provider-credentialing-{Guid.NewGuid():N}";
        
        // Validate at the edge (Fail fast with HTTP 400 - No exceptions thrown)
        var providerIdResult = ProviderId.Create(request.ProviderId);
        if (providerIdResult.IsFailure) return BadRequest(providerIdResult.Error);

        var licenseNumberResult = LicenseNumber.Create(request.LicenseNumber);
        if (licenseNumberResult.IsFailure) return BadRequest(licenseNumberResult.Error);

        var medicalBoardResult = MedicalBoard.Create(request.MedicalBoard);
        if (medicalBoardResult.IsFailure) return BadRequest(medicalBoardResult.Error);

        var expiryDateResult = LicenseExpiryDate.Create(request.ExpiryDate);
        if (expiryDateResult.IsFailure) return BadRequest(expiryDateResult.Error);

        // Map the validated domain values into the Primitive DTO
        var workflowInput = new CredentialingInput(
            providerIdResult.Value.ToString(),
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
                    ["ProviderId"] = request.ProviderId
                }
            });

        return Ok(new { WorkflowId = workflowId, ProviderId = request.ProviderId, Message = "Provider credentialing workflow started" });
    }

    [HttpGet("providers/{providerId}/status")]
    public async Task<IActionResult> GetStatus(string providerId)
    {
        var status = await _statusQuery.GetByProviderIdAsync(providerId);

        if (status == null)
        {
            return Ok(new CredentialEvaluationStatus
            {
                Status = "FetchingLicense",
                Step = 0,
                IsWaitingForManualReview = false
            });
        }

        return Ok(new CredentialEvaluationStatus
        {
            Status = status.Status,
            Step = status.Step,
            IsWaitingForManualReview = status.IsWaitingForManualReview,
            IsCompliant = status.IsCompliant,
            Notes = status.Notes
        });
    }

    [HttpPost("{workflowId}/manual-review")]
    public async Task<IActionResult> CompleteManualReview(string workflowId, [FromBody] ManualReviewRequest request)
    {
        var handle = _temporalClient.GetWorkflowHandle(workflowId);
        await handle.SignalAsync(
            (ProviderCredentialingWorkflow wf) => wf.ManualReviewCompletedAsync(request.IsApproved, request.Notes));

        return Ok(new { Message = "Manual review signal sent" });
    }

    public record StartCredentialingRequest(string ProviderId, string LicenseNumber, string MedicalBoard, DateTimeOffset ExpiryDate, string FirstName, string LastName, string Email, string Specialty);
    public record ManualReviewRequest(bool IsApproved, string? Notes);
    public record CredentialEvaluationStatus
    {
        public string Status { get; init; } = string.Empty;
        public int Step { get; init; }
        public bool IsWaitingForManualReview { get; init; }
        public bool? IsCompliant { get; init; }
        public string? Notes { get; init; }
    }
}
