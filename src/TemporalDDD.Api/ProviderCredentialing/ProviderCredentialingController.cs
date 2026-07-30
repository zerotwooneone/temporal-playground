using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Temporalio.Client;
using TemporalDDD.Application.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Api.ProviderCredentialing;

[ApiController]
[Route("api/[controller]")]
public class ProviderCredentialingController : ControllerBase
{
    private readonly ITemporalClient _temporalClient;
    private readonly ApplicationDbContext _dbContext;

    public ProviderCredentialingController(ITemporalClient temporalClient, ApplicationDbContext dbContext)
    {
        _temporalClient = temporalClient;
        _dbContext = dbContext;
    }

    [HttpPost]
    public async Task<IActionResult> StartCredentialing([FromBody] StartCredentialingRequest request)
    {
        var workflowId = $"provider-credentialing-{request.ProviderId}-{Guid.NewGuid():N}";
        
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
            providerIdResult.Value.Value,
            licenseNumberResult.Value.Value,
            request.MedicalBoard,
            request.ExpiryDate
        );

        // Pass the single JSON-friendly object to Temporal
        await _temporalClient.StartWorkflowAsync(
            (ProviderCredentialingWorkflow wf) => wf.RunAsync(workflowInput),
            new WorkflowOptions
            {
                Id = workflowId,
                TaskQueue = "ONBOARDING_TASK_QUEUE"
            });

        return Ok(new { WorkflowId = workflowId, Message = "Provider credentialing workflow started" });
    }

    [HttpGet("{workflowId}")]
    public async Task<IActionResult> GetStatus(string workflowId)
    {
        // Extract provider ID from workflow ID for prototyping
        // Format: provider-credentialing-{providerId}-{guid}
        var parts = workflowId.Split('-');
        if (parts.Length < 3 || !uint.TryParse(parts[2], out var providerId))
        {
            return BadRequest(new { Error = "Invalid workflow ID format" });
        }

        // Query database for credential evaluation status
        var evaluation = await _dbContext.CredentialEvaluations
            .FirstOrDefaultAsync(e => e.ProviderId == providerId);

        if (evaluation == null)
        {
            return Ok(new CredentialEvaluationStatus
            {
                WorkflowId = workflowId,
                Status = "FetchingLicense",
                Step = 0,
                IsWaitingForManualReview = false
            });
        }

        var status = evaluation.Status.ToString();
        var (step, isWaitingForManualReview) = MapStatusToStep(status);

        return Ok(new CredentialEvaluationStatus
        {
            WorkflowId = workflowId,
            Status = status,
            Step = step,
            IsWaitingForManualReview = isWaitingForManualReview,
            IsCompliant = evaluation.IsCompliant,
            Notes = evaluation.ComplianceNotes
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

    private (int Step, bool IsWaitingForManualReview) MapStatusToStep(string status)
    {
        return status switch
        {
            "Pending" => (0, false),
            "ManualReviewRequired" => (2, true),
            "Approved" => (3, false),
            "Rejected" => (2, false),
            _ => (1, false)
        };
    }

    public record StartCredentialingRequest(uint ProviderId, string LicenseNumber, string MedicalBoard, DateTime ExpiryDate);
    public record ManualReviewRequest(bool IsApproved, string? Notes);
    public record CredentialEvaluationStatus
    {
        public string WorkflowId { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public int Step { get; init; }
        public bool IsWaitingForManualReview { get; init; }
        public bool? IsCompliant { get; init; }
        public string? Notes { get; init; }
    }
}
