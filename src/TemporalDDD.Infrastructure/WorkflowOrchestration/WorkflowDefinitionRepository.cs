using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration.Nodes;
using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public class WorkflowDefinitionRepository : IWorkflowDefinitionRepository
{
    private readonly ApplicationDbContext _dbContext;

    public WorkflowDefinitionRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WorkflowDefinition?> GetByIdAsync(WorkflowDefinitionId id, CancellationToken cancellationToken = default)
    {
        var dbo = await _dbContext.WorkflowDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id.ToString(), cancellationToken);

        if (dbo == null) return null;

        var nodeDbos = await _dbContext.WorkflowNodes
            .AsNoTracking()
            .Where(n => n.WorkflowDefinitionId == dbo.Id)
            .ToListAsync(cancellationToken);

        var transitionDbos = await _dbContext.WorkflowTransitions
            .AsNoTracking()
            .Where(t => t.WorkflowDefinitionId == dbo.Id)
            .ToListAsync(cancellationToken);

        return MapToDomain(dbo, nodeDbos, transitionDbos);
    }

    public async Task SaveAsync(WorkflowDefinition aggregate, CancellationToken cancellationToken = default)
    {
        var id = aggregate.Id.ToString();
        var existing = await _dbContext.WorkflowDefinitions
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

        if (existing == null)
        {
            existing = new WorkflowDefinitionDbo();
            MapToDbo(aggregate, existing);
            _dbContext.WorkflowDefinitions.Add(existing);
        }
        else
        {
            MapToDbo(aggregate, existing);
        }

        // Handle nodes - delete existing and add new
        var existingNodes = await _dbContext.WorkflowNodes
            .Where(n => n.WorkflowDefinitionId == id)
            .ToListAsync(cancellationToken);

        _dbContext.WorkflowNodes.RemoveRange(existingNodes);

        foreach (var node in aggregate.Nodes)
        {
            var nodeDbo = MapNodeToDbo(node, id);
            _dbContext.WorkflowNodes.Add(nodeDbo);
        }

        // Handle transitions - delete existing and add new
        var existingTransitions = await _dbContext.WorkflowTransitions
            .Where(t => t.WorkflowDefinitionId == id)
            .ToListAsync(cancellationToken);

        _dbContext.WorkflowTransitions.RemoveRange(existingTransitions);

        foreach (var transition in aggregate.Transitions)
        {
            var transitionDbo = new WorkflowTransitionDbo
            {
                WorkflowDefinitionId = id,
                SourceNodeId = transition.SourceNodeId.ToString(),
                TargetNodeId = transition.TargetNodeId.ToString(),
                BranchLabel = transition.BranchLabel
            };
            _dbContext.WorkflowTransitions.Add(transitionDbo);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private WorkflowDefinition MapToDomain(WorkflowDefinitionDbo dbo, List<WorkflowNodeDbo> nodeDbos, List<WorkflowTransitionDbo> transitionDbos)
    {
        var id = WorkflowDefinitionId.Create(dbo.Id).Value ?? throw new InvalidOperationException($"Invalid WorkflowDefinitionId in database: {dbo.Id}");
        var publicId = WorkflowDefinitionPublicId.Create(dbo.PublicId).Value ?? throw new InvalidOperationException($"Invalid WorkflowDefinitionPublicId in database: {dbo.PublicId}");
        var creatorId = UserId.Create(dbo.CreatorId).Value ?? throw new InvalidOperationException($"Invalid UserId in database: {dbo.CreatorId}");
        
        var statusResult = WorkflowStatus.FromValue(dbo.Status);
        if (statusResult.IsFailure)
            throw new InvalidOperationException($"Invalid WorkflowStatus in database: {dbo.Status}. {statusResult.Error}");
        var status = statusResult.Value;

        var nodes = nodeDbos.Select(MapDboToNode).ToList();

        var transitions = transitionDbos.Select(t =>
        {
            var sourceNodeId = WorkflowNodeId.Create(t.SourceNodeId).Value ?? throw new InvalidOperationException($"Invalid SourceNodeId in database: {t.SourceNodeId}");
            var targetNodeId = WorkflowNodeId.Create(t.TargetNodeId).Value ?? throw new InvalidOperationException($"Invalid TargetNodeId in database: {t.TargetNodeId}");
            return new WorkflowTransition(sourceNodeId, targetNodeId, t.BranchLabel);
        }).ToList();

        // Parse ClassName from database
        var className = WorkflowClassName.Create(dbo.Name, publicId).Value;

        // Deserialize ExpectedInputs from JSON
        var expectedInputs = JsonSerializer.Deserialize<List<NodeOutputDefinition>>(dbo.ExpectedInputsJson, GetJsonOptions()) ?? new List<NodeOutputDefinition>();

        // Use internal constructor for rehydration
        var workflow = new WorkflowDefinition(
            id: id,
            publicId: publicId,
            creatorId: creatorId,
            name: dbo.Name,
            className: className,
            status: status,
            flowJson: dbo.FlowJson,
            nodes: nodes,
            transitions: transitions
        );

        // Set ExpectedInputs using the public method
        workflow.UpdateWorkflowInputs(expectedInputs);

        return workflow;
    }

    private WorkflowNode MapDboToNode(WorkflowNodeDbo dbo)
    {
        var nodeId = WorkflowNodeId.Create(dbo.Id).Value ?? throw new InvalidOperationException($"Invalid WorkflowNodeId in database: {dbo.Id}");

        var nodeTypeResult = NodeType.FromValue(dbo.NodeType);
        if (nodeTypeResult.IsFailure)
            throw new InvalidOperationException($"Invalid NodeType in database: {dbo.NodeType}. {nodeTypeResult.Error}");
        var nodeType = nodeTypeResult.Value;

        // Deserialize JSON columns
        var inputDefinitions = JsonSerializer.Deserialize<List<NodeInputDefinition>>(dbo.InputDefinitionsJson, GetJsonOptions()) ?? new List<NodeInputDefinition>();
        var outputDefinitions = JsonSerializer.Deserialize<List<NodeOutputDefinition>>(dbo.OutputDefinitionsJson, GetJsonOptions()) ?? new List<NodeOutputDefinition>();
        var inputBindings = JsonSerializer.Deserialize<List<ParameterBinding>>(dbo.InputBindingsJson, GetJsonOptions()) ?? new List<ParameterBinding>();

        return nodeType switch
        {
            var t when t == NodeType.Start => MapStartNode(dbo, nodeId, inputDefinitions, outputDefinitions, inputBindings),
            var t when t == NodeType.End => new EndWorkflowNode(nodeId, dbo.Name, dbo.BusinessNotes, dbo.IsConfigured),
            var t when t == NodeType.Api => MapApiNode(dbo, nodeId, inputDefinitions, outputDefinitions, inputBindings),
            var t when t == NodeType.Notification => MapNotificationNode(dbo, nodeId, inputDefinitions, outputDefinitions, inputBindings),
            _ => throw new InvalidOperationException($"Unsupported NodeType in database: {dbo.NodeType}")
        };
    }

    private StartWorkflowNode MapStartNode(WorkflowNodeDbo dbo, WorkflowNodeId nodeId, List<NodeInputDefinition> inputDefinitions, List<NodeOutputDefinition> outputDefinitions, List<ParameterBinding> inputBindings)
    {
        var node = new StartWorkflowNode(nodeId, dbo.Name, dbo.BusinessNotes, dbo.IsConfigured);
        node.ConfigureOutputs(outputDefinitions);
        return node;
    }

    private ApiWorkflowNode MapApiNode(WorkflowNodeDbo dbo, WorkflowNodeId nodeId, List<NodeInputDefinition> inputDefinitions, List<NodeOutputDefinition> outputDefinitions, List<ParameterBinding> inputBindings)
    {
        if (dbo is not ApiWorkflowNodeDbo apiDbo)
            throw new InvalidOperationException($"Expected ApiWorkflowNodeDbo but got {dbo.GetType().Name}");

        // Reconstruct RetryPolicy
        RetryPolicy? retryPolicy = null;
        if (apiDbo.RetryPolicyMaxAttempts.HasValue && apiDbo.RetryPolicyBackoffCoefficient.HasValue)
        {
            var retryResult = RetryPolicy.Create(apiDbo.RetryPolicyMaxAttempts.Value, apiDbo.RetryPolicyBackoffCoefficient.Value);
            if (retryResult.IsSuccess)
                retryPolicy = retryResult.Value;
        }

        // Reconstruct ContractMapping
        ContractMapping? contractMapping = null;
        if (apiDbo.ContractMappingConvertXmlToJson.HasValue)
        {
            var mappingResult = ContractMapping.Create(
                apiDbo.ContractMappingConvertXmlToJson.Value,
                apiDbo.ContractMappingQueryParameters,
                apiDbo.ContractMappingRequestMapping,
                apiDbo.ContractMappingResponseMapping);
            if (mappingResult.IsSuccess)
                contractMapping = mappingResult.Value;
        }

        var node = new ApiWorkflowNode(
            id: nodeId,
            name: dbo.Name,
            businessNotes: dbo.BusinessNotes,
            isConfigured: dbo.IsConfigured,
            endpointUrl: apiDbo.EndpointUrl,
            authToken: apiDbo.AuthToken,
            retryPolicy: retryPolicy,
            contractMapping: contractMapping
        );

        // Apply deserialized data
        node.UpdateInputBindings(inputBindings);
        if (outputDefinitions.Any())
        {
            node.ConfigureResponseSchema(outputDefinitions);
        }

        return node;
    }

    private NotificationWorkflowNode MapNotificationNode(WorkflowNodeDbo dbo, WorkflowNodeId nodeId, List<NodeInputDefinition> inputDefinitions, List<NodeOutputDefinition> outputDefinitions, List<ParameterBinding> inputBindings)
    {
        if (dbo is not NotificationWorkflowNodeDbo notificationDbo)
            throw new InvalidOperationException($"Expected NotificationWorkflowNodeDbo but got {dbo.GetType().Name}");

        var node = new NotificationWorkflowNode(
            id: nodeId,
            name: dbo.Name,
            businessNotes: dbo.BusinessNotes,
            isConfigured: dbo.IsConfigured,
            messageTemplate: notificationDbo.MessageTemplate
        );

        // Apply deserialized data
        node.UpdateInputBindings(inputBindings);

        return node;
    }

    private void MapToDbo(WorkflowDefinition workflow, WorkflowDefinitionDbo dbo)
    {
        dbo.Id = workflow.Id.ToString();
        dbo.PublicId = workflow.PublicId.ToString();
        dbo.CreatorId = workflow.CreatorId.ToString();
        dbo.Name = workflow.Name;
        dbo.ClassName = workflow.ClassName.Value;
        dbo.Status = workflow.Status.Value;
        dbo.FlowJson = workflow.FlowJson;
        
        // Serialize ExpectedInputs to JSON
        dbo.ExpectedInputsJson = JsonSerializer.Serialize(workflow.ExpectedInputs, GetJsonOptions());
    }

    private WorkflowNodeDbo MapNodeToDbo(WorkflowNode node, string workflowDefinitionId)
    {
        return node switch
        {
            StartWorkflowNode startNode => new StartWorkflowNodeDbo
            {
                Id = startNode.Id.ToString(),
                WorkflowDefinitionId = workflowDefinitionId,
                NodeType = startNode.Type.Value,
                Name = startNode.Name,
                BusinessNotes = startNode.BusinessNotes,
                IsConfigured = startNode.IsConfigured,
                InputDefinitionsJson = JsonSerializer.Serialize(startNode.InputDefinitions, GetJsonOptions()),
                OutputDefinitionsJson = JsonSerializer.Serialize(startNode.OutputDefinitions, GetJsonOptions()),
                InputBindingsJson = JsonSerializer.Serialize(startNode.InputBindings, GetJsonOptions())
            },
            EndWorkflowNode endNode => new EndWorkflowNodeDbo
            {
                Id = endNode.Id.ToString(),
                WorkflowDefinitionId = workflowDefinitionId,
                NodeType = endNode.Type.Value,
                Name = endNode.Name,
                BusinessNotes = endNode.BusinessNotes,
                IsConfigured = endNode.IsConfigured,
                InputDefinitionsJson = JsonSerializer.Serialize(endNode.InputDefinitions, GetJsonOptions()),
                OutputDefinitionsJson = JsonSerializer.Serialize(endNode.OutputDefinitions, GetJsonOptions()),
                InputBindingsJson = JsonSerializer.Serialize(endNode.InputBindings, GetJsonOptions())
            },
            ApiWorkflowNode apiNode => MapApiNodeToDbo(apiNode, workflowDefinitionId),
            NotificationWorkflowNode notificationNode => MapNotificationNodeToDbo(notificationNode, workflowDefinitionId),
            _ => throw new InvalidOperationException($"Unsupported WorkflowNode type: {node.GetType().Name}")
        };
    }

    private ApiWorkflowNodeDbo MapApiNodeToDbo(ApiWorkflowNode node, string workflowDefinitionId)
    {
        return new ApiWorkflowNodeDbo
        {
            Id = node.Id.ToString(),
            WorkflowDefinitionId = workflowDefinitionId,
            NodeType = node.Type.Value,
            Name = node.Name,
            BusinessNotes = node.BusinessNotes,
            IsConfigured = node.IsConfigured,
            EndpointUrl = node.EndpointUrl,
            AuthToken = node.AuthToken,
            RetryPolicyMaxAttempts = node.RetryPolicy?.MaxAttempts,
            RetryPolicyBackoffCoefficient = node.RetryPolicy?.BackoffCoefficient,
            ContractMappingConvertXmlToJson = node.ContractMapping?.ConvertXmlToJson,
            ContractMappingQueryParameters = node.ContractMapping?.QueryParameters,
            ContractMappingRequestMapping = node.ContractMapping?.RequestMapping,
            ContractMappingResponseMapping = node.ContractMapping?.ResponseMapping,
            InputDefinitionsJson = JsonSerializer.Serialize(node.InputDefinitions, GetJsonOptions()),
            OutputDefinitionsJson = JsonSerializer.Serialize(node.OutputDefinitions, GetJsonOptions()),
            InputBindingsJson = JsonSerializer.Serialize(node.InputBindings, GetJsonOptions())
        };
    }

    private NotificationWorkflowNodeDbo MapNotificationNodeToDbo(NotificationWorkflowNode node, string workflowDefinitionId)
    {
        return new NotificationWorkflowNodeDbo
        {
            Id = node.Id.ToString(),
            WorkflowDefinitionId = workflowDefinitionId,
            NodeType = node.Type.Value,
            Name = node.Name,
            BusinessNotes = node.BusinessNotes,
            IsConfigured = node.IsConfigured,
            MessageTemplate = node.MessageTemplate,
            InputDefinitionsJson = JsonSerializer.Serialize(node.InputDefinitions, GetJsonOptions()),
            OutputDefinitionsJson = JsonSerializer.Serialize(node.OutputDefinitions, GetJsonOptions()),
            InputBindingsJson = JsonSerializer.Serialize(node.InputBindings, GetJsonOptions())
        };
    }

    private static JsonSerializerOptions GetJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
        
        // Configure polymorphic type discriminator for WorkflowDataType
        options.TypeInfoResolver = new PolymorphicTypeResolver();
        
        return options;
    }
}
