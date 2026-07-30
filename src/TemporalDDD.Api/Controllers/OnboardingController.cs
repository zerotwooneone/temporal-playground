using Microsoft.AspNetCore.Mvc;
using Temporalio.Client;
using TemporalDDD.Application.ProviderOnboarding;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Api.Controllers;

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
        
        // Convert primitives to domain types
        var providerId = ProviderId.Create(request.ProviderId);
        var licenseNumber = LicenseNumber.Create(request.LicenseNumber);
        
        await _temporalClient.ExecuteWorkflowAsync(
            (IProviderOnboardingWorkflow wf) => wf.RunAsync(providerId, licenseNumber),
            new WorkflowOptions
            {
                Id = workflowId,
                TaskQueue = "ONBOARDING_TASK_QUEUE"
            });

        return Ok(new { WorkflowId = workflowId, Message = "Provider onboarding workflow started" });
    }

    public record OnboardingRequest(uint ProviderId, string LicenseNumber);
}
