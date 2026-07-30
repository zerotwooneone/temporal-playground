using Microsoft.AspNetCore.Mvc;
using Temporalio.Client;
using TemporalDDD.Application.ProviderOnboarding;
using TemporalDDD.Domain.ProviderCredentialing;
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
        
        // Convert primitives to domain types using Result<T>
        var providerIdResult = ProviderId.Create(request.ProviderId);
        var licenseNumberResult = LicenseNumber.Create(request.LicenseNumber);
        
        // Check for validation failures
        if (providerIdResult.IsFailure)
            return BadRequest(new { Error = providerIdResult.Error });
        
        if (licenseNumberResult.IsFailure)
            return BadRequest(new { Error = licenseNumberResult.Error });
        
        await _temporalClient.ExecuteWorkflowAsync(
            (IProviderOnboardingWorkflow wf) => wf.RunAsync(providerIdResult.Value, licenseNumberResult.Value),
            new WorkflowOptions
            {
                Id = workflowId,
                TaskQueue = "ONBOARDING_TASK_QUEUE"
            });

        return Ok(new { WorkflowId = workflowId, Message = "Provider onboarding workflow started" });
    }

    public record OnboardingRequest(uint ProviderId, string LicenseNumber);
}
