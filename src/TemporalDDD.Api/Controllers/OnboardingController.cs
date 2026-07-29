using Microsoft.AspNetCore.Mvc;
using Temporalio.Client;
using TemporalDDD.Application.ProviderOnboarding;

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
        
        await _temporalClient.ExecuteWorkflowAsync(
            (IProviderOnboardingWorkflow wf) => wf.RunAsync(request.ProviderId, request.LicenseNumber),
            new WorkflowOptions
            {
                Id = workflowId,
                TaskQueue = "ONBOARDING_TASK_QUEUE"
            });

        return Ok(new { WorkflowId = workflowId, Message = "Provider onboarding workflow started" });
    }

    public record OnboardingRequest(uint ProviderId, string LicenseNumber);
}
