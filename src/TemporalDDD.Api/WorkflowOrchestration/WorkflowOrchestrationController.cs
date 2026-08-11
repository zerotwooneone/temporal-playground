using Microsoft.AspNetCore.Mvc;
using Temporalio.Client;
using TemporalDDD.Application.WorkflowOrchestration;
using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Infrastructure.Generators;

namespace TemporalDDD.Api.WorkflowOrchestration;

[ApiController]
[Route("api/workflows")]
public class WorkflowOrchestrationController : ControllerBase
{
    private readonly ITemporalClient _temporalClient;
    private readonly IWorkflowDefinitionQuery _query;
    private readonly IWorkflowCodeGeneratorService _codeGeneratorService;
    private readonly IConfiguration _configuration;
    private readonly IWorkflowDefinitionRepository _repository;

    public WorkflowOrchestrationController(
        ITemporalClient temporalClient,
        IWorkflowDefinitionQuery query,
        IWorkflowCodeGeneratorService codeGeneratorService,
        IConfiguration configuration,
        IWorkflowDefinitionRepository repository)
    {
        _temporalClient = temporalClient;
        _query = query;
        _codeGeneratorService = codeGeneratorService;
        _configuration = configuration;
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllWorkflows(CancellationToken cancellationToken = default)
    {
        var workflows = await _query.GetAllWorkflowsAsync(cancellationToken);
        return Ok(workflows);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetWorkflowById(string id, CancellationToken cancellationToken = default)
    {
        var workflow = await _query.GetWorkflowByIdAsync(id, cancellationToken);
        if (workflow == null)
            return NotFound($"Workflow with ID '{id}' not found");
        return Ok(workflow);
    }

    [HttpPost]
    public async Task<IActionResult> CreateWorkflowDraft([FromBody] CreateWorkflowRequest request, CancellationToken cancellationToken = default)
    {
        // Validate CreatorId at the edge using domain value type
        var creatorIdResult = UserId.Create(request.CreatorId);
        if (creatorIdResult.IsFailure)
            return BadRequest(creatorIdResult.Error);

        // Validate Name is not empty
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required and cannot be empty.");

        var workflowId = $"WFLId{Guid.NewGuid()}";

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
        // Validate workflow ID using domain value type
        var workflowIdResult = WorkflowDefinitionId.Create(id);
        if (workflowIdResult.IsFailure)
            return BadRequest(workflowIdResult.Error);

        // Validate each node's ID and NodeType using domain value types (lightweight in-memory checks)
        foreach (var node in request.Nodes)
        {
            var nodeIdResult = WorkflowNodeId.Create(node.Id);
            if (nodeIdResult.IsFailure)
                return BadRequest($"Invalid node ID '{node.Id}': {nodeIdResult.Error}");

            var nodeTypeResult = NodeType.FromValue(node.NodeType);
            if (nodeTypeResult.IsFailure)
                return BadRequest($"Invalid NodeType {node.NodeType} for node '{node.Id}': {nodeTypeResult.Error}");
        }

        // Map API DTOs to Application DTOs
        var applicationNodeDtos = request.Nodes.Select(node => new Application.WorkflowOrchestration.WorkflowNodeDto(
            Id: node.Id,
            NodeType: node.NodeType,
            Name: node.Name,
            BusinessNotes: node.BusinessNotes,
            IsConfigured: node.IsConfigured,
            EndpointUrl: node.EndpointUrl,
            AuthToken: node.AuthToken,
            RetryPolicyMaxAttempts: node.RetryPolicyMaxAttempts,
            RetryPolicyBackoffCoefficient: node.RetryPolicyBackoffCoefficient,
            ContractMappingConvertXmlToJson: node.ContractMappingConvertXmlToJson,
            ContractMappingQueryParameters: node.ContractMappingQueryParameters,
            ContractMappingRequestMapping: node.ContractMappingRequestMapping,
            ContractMappingResponseMapping: node.ContractMappingResponseMapping,
            MessageTemplate: node.MessageTemplate
        )).ToList();

        var applicationTransitionDtos = request.Transitions.Select(t => new Application.WorkflowOrchestration.WorkflowTransitionDto(
            SourceNodeId: t.SourceNodeId,
            TargetNodeId: t.TargetNodeId
        )).ToList();

        var input = new UpdateWorkflowNodesInput(id, applicationNodeDtos, applicationTransitionDtos);

        try
        {
            await _temporalClient.StartWorkflowAsync(
                (UpdateWorkflowNodesWorkflow wf) => wf.RunAsync(input),
                new WorkflowOptions
                {
                    Id = $"update-nodes-{id}-{Guid.NewGuid():N}",
                    TaskQueue = "WORKFLOW_ORCHESTRATION_TASK_QUEUE",
                    Memo = new Dictionary<string, object>
                    {
                        ["WorkflowId"] = id
                    }
                });

            return Ok(new { message = "Workflow update started" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id}/publish")]
    public async Task<IActionResult> PublishWorkflow(string id, CancellationToken cancellationToken = default)
    {
        // Validate workflow ID using domain value type
        var workflowIdResult = WorkflowDefinitionId.Create(id);
        if (workflowIdResult.IsFailure)
            return BadRequest(workflowIdResult.Error);

        // Retrieve the workflow definition from the database
        var workflowDefinition = await _repository.GetByIdAsync(workflowIdResult.Value, cancellationToken);
        if (workflowDefinition == null)
            return NotFound($"Workflow with ID '{id}' not found");

        // Check if workflow is in Approved status
        if (workflowDefinition.Status != WorkflowStatus.Approved)
            return BadRequest($"Workflow must be in Approved status before publishing. Current status: {workflowDefinition.Status}");

        // Get the worker project path from configuration
        var workerProjectPath = _configuration["WorkerProjectPath"];
        if (string.IsNullOrWhiteSpace(workerProjectPath))
            return BadRequest("WorkerProjectPath is not configured in appsettings.json");

        try
        {
            // Generate the workflow class
            var code = await _codeGeneratorService.GenerateWorkflowClassAsync(workflowDefinition, workerProjectPath);

            // Update the workflow status to Published
            workflowDefinition.Publish();
            await _repository.SaveAsync(workflowDefinition, cancellationToken);

            return Ok(new { message = "Workflow published successfully", className = workflowDefinition.ClassName.Value });
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

public record WorkflowTransitionDto(
    string SourceNodeId,
    string TargetNodeId);

public record UpdateWorkflowNodesRequest(
    string WorkflowId,
    List<WorkflowNodeDto> Nodes,
    List<WorkflowTransitionDto> Transitions);
