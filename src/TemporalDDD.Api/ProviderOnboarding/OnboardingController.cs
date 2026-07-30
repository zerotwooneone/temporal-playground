using Microsoft.AspNetCore.Mvc;
using Temporalio.Client;
using TemporalDDD.Application.ProviderOnboarding;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Api.ProviderOnboarding;

[ApiController]
[Route("api/[controller]")]
public class OnboardingController : ControllerBase
{
    private readonly ITemporalClient _temporalClient;

    public OnboardingController(ITemporalClient temporalClient)
    {
        _temporalClient = temporalClient;
    }

    [HttpPost]
    public async Task<IActionResult> StartOnboarding([FromBody] OnboardingRequest request)
    {
        var workflowId = $"provider-onboarding-{request.ProviderId}";
        
        // Validate at the edge (Fail fast with HTTP 400 - No exceptions thrown)
        var providerIdResult = ProviderId.Create(request.ProviderId);
        if (providerIdResult.IsFailure) return BadRequest(providerIdResult.Error);

        var licenseNumberResult = LicenseNumber.Create(request.LicenseNumber);
        if (licenseNumberResult.IsFailure) return BadRequest(licenseNumberResult.Error);

        // Map the validated domain values into the Primitive DTO
        var workflowInput = new OnboardingInput(
            providerIdResult.Value.ToString(),
            licenseNumberResult.Value.Value
        );

        // Pass the single JSON-friendly object to Temporal
        await _temporalClient.ExecuteWorkflowAsync(
            (IProviderOnboardingWorkflow wf) => wf.RunAsync(workflowInput),
            new WorkflowOptions
            {
                Id = workflowId,
                TaskQueue = "ONBOARDING_TASK_QUEUE"
            });

        return Ok(new { WorkflowId = workflowId, Message = "Provider onboarding workflow started" });
    }

    public record OnboardingRequest(string ProviderId, string LicenseNumber);
}
