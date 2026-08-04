using Microsoft.AspNetCore.Mvc;
using Temporalio.Client;
using TemporalDDD.Application.WorkflowOrchestration;
using TemporalDDD.Domain.IdentityAndAccess;

namespace TemporalDDD.Api.WorkflowOrchestration;

[ApiController]
[Route("api/workflows")]
public class WorkflowOrchestrationController : ControllerBase
{
    private readonly ITemporalClient _temporalClient;
    private readonly IWorkflowDefinitionQuery _query;

    public WorkflowOrchestrationController(ITemporalClient temporalClient, IWorkflowDefinitionQuery query)
    {
        _temporalClient = temporalClient;
        _query = query;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllWorkflows(CancellationToken cancellationToken = default)
    {
        var workflows = await _query.GetAllWorkflowsAsync(cancellationToken);
        return Ok(workflows);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWorkflowDraft([FromBody] CreateWorkflowRequest request, CancellationToken cancellationToken = default)
    {
        // Validate CreatorId at the edge
        var creatorIdResult = UserId.Create(request.CreatorId);
        if (creatorIdResult.IsFailure)
            return BadRequest(creatorIdResult.Error);

        var workflowId = $"workflow-draft-{Guid.NewGuid():N}";

        var workflowInput = new CreateWorkflowDraftInput(
            CreatorId: request.CreatorId,
            Name: request.Name
        );

        try
        {
            await _temporalClient.StartWorkflowAsync(
                (CreateWorkflowDraftWorkflow wf) => wf.RunAsync(workflowInput),
                new WorkflowOptions
                {
                    Id = workflowId,
                    TaskQueue = "WORKFLOW_ORCHESTRATION_TASK_QUEUE",
                    Memo = new Dictionary<string, object>
                    {
                        ["CreatorId"] = request.CreatorId
                    }
                });

            return Ok(new CreateWorkflowResponse(workflowId, "Workflow draft creation started"));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public record CreateWorkflowRequest(
    string CreatorId,
    string Name);

public record CreateWorkflowResponse(
    string WorkflowId,
    string Message);
