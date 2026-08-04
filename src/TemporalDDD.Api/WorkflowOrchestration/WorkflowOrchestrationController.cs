using Microsoft.AspNetCore.Mvc;
using Temporalio.Client;
using TemporalDDD.Application.WorkflowOrchestration;
using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration.Nodes;
using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;
using TemporalDDD.Infrastructure.WorkflowOrchestration;

namespace TemporalDDD.Api.WorkflowOrchestration;

[ApiController]
[Route("api/workflows")]
public class WorkflowOrchestrationController : ControllerBase
{
    private readonly ITemporalClient _temporalClient;
    private readonly IWorkflowDefinitionQuery _query;
    private readonly IWorkflowDefinitionRepository _repository;

    public WorkflowOrchestrationController(ITemporalClient temporalClient, IWorkflowDefinitionQuery query, IWorkflowDefinitionRepository repository)
    {
        _temporalClient = temporalClient;
        _query = query;
        _repository = repository;
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

    [HttpPut("{id}/nodes")]
    public async Task<IActionResult> UpdateWorkflowNodes(string id, [FromBody] UpdateWorkflowNodesRequest request, CancellationToken cancellationToken = default)
    {
        // Validate workflow exists
        var workflowIdResult = WorkflowDefinitionId.Create(id);
        if (workflowIdResult.IsFailure)
            return BadRequest(workflowIdResult.Error);

        var workflow = await _repository.GetByIdAsync(workflowIdResult.Value, cancellationToken);
        if (workflow == null)
            return NotFound($"Workflow with ID {id} not found");

        // Map DTOs to domain nodes
        var domainNodes = new List<WorkflowNode>();
        foreach (var nodeDto in request.Nodes)
        {
            var nodeTypeResult = NodeType.FromValue(nodeDto.NodeType);
            if (nodeTypeResult.IsFailure)
                return BadRequest($"Invalid NodeType for node {nodeDto.Id}: {nodeTypeResult.Error}");

            var nodeIdResult = WorkflowNodeId.Create(nodeDto.Id);
            if (nodeIdResult.IsFailure)
                return BadRequest($"Invalid NodeId for node {nodeDto.Id}: {nodeIdResult.Error}");

            WorkflowNode domainNode = nodeTypeResult.Value.Value switch
            {
                1 => ApiWorkflowNode.CreateStub(nodeDto.Name, nodeDto.BusinessNotes),
                2 => NotificationWorkflowNode.CreateStub(nodeDto.Name, nodeDto.BusinessNotes),
                _ => throw new InvalidOperationException($"Unsupported NodeType: {nodeTypeResult.Value.Name}")
            };

            // Update business intent if provided
            if (nodeDto.BusinessNotes != null || nodeDto.Name != null)
            {
                domainNode.UpdateBusinessIntent(nodeDto.Name, nodeDto.BusinessNotes);
            }

            // Configure API node specifics
            if (nodeTypeResult.Value.Value == 1 && domainNode is ApiWorkflowNode apiNode)
            {
                var retryPolicy = RetryPolicy.Create(
                    nodeDto.RetryPolicyMaxAttempts ?? 3,
                    nodeDto.RetryPolicyBackoffCoefficient ?? 2
                ).Value;

                var contractMapping = ContractMapping.Create(
                    nodeDto.ContractMappingConvertXmlToJson ?? false,
                    nodeDto.ContractMappingQueryParameters ?? string.Empty,
                    nodeDto.ContractMappingRequestMapping ?? string.Empty,
                    nodeDto.ContractMappingResponseMapping ?? string.Empty
                ).Value;

                apiNode.ConfigureTechnicalDetails(
                    nodeDto.EndpointUrl ?? string.Empty,
                    nodeDto.AuthToken,
                    retryPolicy,
                    contractMapping
                );
            }

            // Configure notification node specifics
            if (nodeTypeResult.Value.Value == 2 && domainNode is NotificationWorkflowNode notificationNode)
            {
                notificationNode.ConfigureTechnicalDetails(nodeDto.MessageTemplate ?? string.Empty);
            }

            domainNodes.Add(domainNode);
        }

        // Update workflow with new nodes
        var updatedWorkflow = workflow.UpdateNodes(domainNodes);
        await _repository.SaveAsync(updatedWorkflow, cancellationToken);

        return Ok(new { message = "Workflow nodes saved successfully" });
    }
}

public record CreateWorkflowRequest(
    string CreatorId,
    string Name);

public record CreateWorkflowResponse(
    string WorkflowId,
    string Message);

public record WorkflowNodeDto(
    string Id,
    int NodeType,
    string Name,
    string? BusinessNotes,
    bool IsConfigured,
    // Api Node properties
    string? EndpointUrl,
    string? AuthToken,
    int? RetryPolicyMaxAttempts,
    int? RetryPolicyBackoffCoefficient,
    bool? ContractMappingConvertXmlToJson,
    string? ContractMappingQueryParameters,
    string? ContractMappingRequestMapping,
    string? ContractMappingResponseMapping,
    // Notification Node properties
    string? MessageTemplate
);

public record UpdateWorkflowNodesRequest(
    string WorkflowId,
    List<WorkflowNodeDto> Nodes);
