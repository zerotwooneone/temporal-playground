using Microsoft.AspNetCore.Mvc;
using TemporalDDD.Application.WorkflowOrchestration;
using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration;

namespace TemporalDDD.Api.WorkflowOrchestration;

[ApiController]
[Route("api/workflows")]
public class WorkflowOrchestrationController : ControllerBase
{
    private readonly IWorkflowDefinitionQuery _query;
    private readonly IWorkflowCodeGeneratorService _codeGeneratorService;
    private readonly IConfiguration _configuration;
    private readonly IWorkflowDefinitionRepository _repository;
    private readonly IWorkflowNodeService _workflowNodeService;

    public WorkflowOrchestrationController(
        IWorkflowDefinitionQuery query,
        IWorkflowCodeGeneratorService codeGeneratorService,
        IConfiguration configuration,
        IWorkflowDefinitionRepository repository,
        IWorkflowNodeService workflowNodeService)
    {
        _query = query;
        _codeGeneratorService = codeGeneratorService;
        _configuration = configuration;
        _repository = repository;
        _workflowNodeService = workflowNodeService;
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
        var publicIdResult = WorkflowDefinitionPublicId.Create(id);
        if (publicIdResult.IsFailure)
            return BadRequest(publicIdResult.Error);

        var workflow = await _query.GetWorkflowByPublicIdAsync(publicIdResult.Value, cancellationToken);
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

        var publicId = WorkflowDefinitionPublicId.New();

        // Create domain entity using factory
        var workflow = WorkflowDefinition.Create(
            creatorId: creatorIdResult.Value,
            name: request.Name,
            initialJson: "{}",
            publicId: publicId);

        // Save to database using repository
        await _repository.SaveAsync(workflow, cancellationToken);

        return Ok(new { publicId = publicId.ToString() });
    }

    [HttpPut("{id}/nodes")]
    public async Task<IActionResult> UpdateWorkflowNodes(string id, [FromBody] UpdateWorkflowNodesRequest request, CancellationToken cancellationToken = default)
    {
        // Validate workflow ID using domain value type
        var publicIdResult = WorkflowDefinitionPublicId.Create(id);
        if (publicIdResult.IsFailure)
            return BadRequest(publicIdResult.Error);

        // Resolve WorkflowDefinitionId from PublicId
        var workflowDefinitionId = await _query.GetWorkflowDefinitionIdByPublicIdAsync(publicIdResult.Value, cancellationToken);
        if (workflowDefinitionId == null)
            return NotFound($"Workflow with PublicId '{id}' not found");

        // Validate flowJson - lightweight sanity checks
        if (string.IsNullOrWhiteSpace(request.FlowJson))
            return BadRequest("FlowJson is required");

        // Check string length to prevent excessively large payloads
        const int maxFlowJsonLength = 1_048_576; // 1MB
        if (request.FlowJson.Length > maxFlowJsonLength)
            return BadRequest($"FlowJson exceeds maximum length of {maxFlowJsonLength} characters");

        try
        {
            var flowData = System.Text.Json.JsonDocument.Parse(request.FlowJson);
            if (!flowData.RootElement.TryGetProperty("nodes", out _))
                return BadRequest("FlowJson must contain a 'nodes' array");
            if (!flowData.RootElement.TryGetProperty("edges", out _))
                return BadRequest("FlowJson must contain an 'edges' array");
        }
        catch (System.Text.Json.JsonException)
        {
            return BadRequest("FlowJson is not valid JSON");
        }

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
            TargetNodeId: t.TargetNodeId,
            SourcePort: t.SourcePort
        )).ToList();

        var input = new UpdateWorkflowNodesInput(workflowDefinitionId.ToString(), request.FlowJson, applicationNodeDtos, applicationTransitionDtos);

        try
        {
            await _workflowNodeService.UpdateNodesAsync(workflowDefinitionId, request.FlowJson, input, cancellationToken);
            return Ok(new { message = "Workflow updated successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{publicId}/submit")]
    public async Task<IActionResult> SubmitForReview(string publicId, CancellationToken cancellationToken = default)
    {
        // Validate workflow ID using domain value type
        var publicIdResult = WorkflowDefinitionPublicId.Create(publicId);
        if (publicIdResult.IsFailure)
            return BadRequest(publicIdResult.Error);

        // Resolve WorkflowDefinitionId from PublicId
        var workflowDefinitionId = await _query.GetWorkflowDefinitionIdByPublicIdAsync(publicIdResult.Value, cancellationToken);
        if (workflowDefinitionId == null)
            return NotFound($"Workflow with PublicId '{publicId}' not found");

        // Retrieve the workflow definition from the database
        var workflowDefinition = await _repository.GetByIdAsync(workflowDefinitionId, cancellationToken);
        if (workflowDefinition == null)
            return NotFound($"Workflow with ID '{publicId}' not found");

        try
        {
            // Submit workflow for review
            workflowDefinition.SubmitForReview();
            await _repository.SaveAsync(workflowDefinition, cancellationToken);

            return Ok(new { message = "Workflow submitted for review successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{publicId}/approve")]
    public async Task<IActionResult> ApproveWorkflow(string publicId, [FromBody] ApproveWorkflowRequest request, CancellationToken cancellationToken = default)
    {
        // Validate workflow ID using domain value type
        var publicIdResult = WorkflowDefinitionPublicId.Create(publicId);
        if (publicIdResult.IsFailure)
            return BadRequest(publicIdResult.Error);

        // Validate ReviewerId at the edge using domain value type
        var reviewerIdResult = UserId.Create(request.ReviewerId);
        if (reviewerIdResult.IsFailure)
            return BadRequest(reviewerIdResult.Error);

        // Resolve WorkflowDefinitionId from PublicId
        var workflowDefinitionId = await _query.GetWorkflowDefinitionIdByPublicIdAsync(publicIdResult.Value, cancellationToken);
        if (workflowDefinitionId == null)
            return NotFound($"Workflow with PublicId '{publicId}' not found");

        // Retrieve the workflow definition from the database
        var workflowDefinition = await _repository.GetByIdAsync(workflowDefinitionId, cancellationToken);
        if (workflowDefinition == null)
            return NotFound($"Workflow with ID '{publicId}' not found");

        try
        {
            // Approve workflow
            workflowDefinition.Approve(reviewerIdResult.Value);
            await _repository.SaveAsync(workflowDefinition, cancellationToken);

            return Ok(new { message = "Workflow approved successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{publicId}/publish")]
    public async Task<IActionResult> PublishWorkflow(string publicId, CancellationToken cancellationToken = default)
    {
        // Validate workflow ID using domain value type
        var publicIdResult = WorkflowDefinitionPublicId.Create(publicId);
        if (publicIdResult.IsFailure)
            return BadRequest(publicIdResult.Error);

        // Resolve WorkflowDefinitionId from PublicId
        var workflowDefinitionId = await _query.GetWorkflowDefinitionIdByPublicIdAsync(publicIdResult.Value, cancellationToken);
        if (workflowDefinitionId == null)
            return NotFound($"Workflow with PublicId '{publicId}' not found");

        // Retrieve the workflow definition from the database
        var workflowDefinition = await _repository.GetByIdAsync(workflowDefinitionId, cancellationToken);
        if (workflowDefinition == null)
            return NotFound($"Workflow with ID '{publicId}' not found");

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
    string TargetNodeId,
    string? SourcePort = null);

public record UpdateWorkflowNodesRequest(
    string FlowJson,
    List<WorkflowNodeDto> Nodes,
    List<WorkflowTransitionDto> Transitions);

public record ApproveWorkflowRequest(
    string ReviewerId);
