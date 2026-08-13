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
                SourcePort = transition.SourcePort
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
            return new WorkflowTransition(sourceNodeId, targetNodeId, t.SourcePort);
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

        // Deserialize JSON columns using DTOs
        var inputDefinitionDtos = JsonSerializer.Deserialize<List<NodeInputDefinitionDto>>(dbo.InputDefinitionsJson, GetJsonOptions()) ?? new List<NodeInputDefinitionDto>();
        var outputDefinitionDtos = JsonSerializer.Deserialize<List<NodeOutputDefinitionDto>>(dbo.OutputDefinitionsJson, GetJsonOptions()) ?? new List<NodeOutputDefinitionDto>();
        var inputBindings = JsonSerializer.Deserialize<List<ParameterBinding>>(dbo.InputBindingsJson, GetJsonOptions()) ?? new List<ParameterBinding>();

        // Convert DTOs to domain objects
        var inputDefinitions = inputDefinitionDtos.Select(NodeContractsDtoMapper.ToDomain).ToList();
        var outputDefinitions = outputDefinitionDtos.Select(NodeContractsDtoMapper.ToDomain).ToList();

        return nodeType switch
        {
            var t when t == NodeType.Start => MapStartNode(dbo, nodeId, inputDefinitions, outputDefinitions, inputBindings),
            var t when t == NodeType.End => new EndWorkflowNode(nodeId, dbo.Name, dbo.BusinessNotes, dbo.IsConfigured),
            var t when t == NodeType.Api => MapApiNode(dbo, nodeId, inputDefinitions, outputDefinitions, inputBindings),
            var t when t == NodeType.Notification => MapNotificationNode(dbo, nodeId, inputDefinitions, outputDefinitions, inputBindings),
            var t when t == NodeType.HumanTask => MapHumanTaskNode(dbo, nodeId, inputDefinitions, outputDefinitions, inputBindings),
            var t when t == NodeType.Decision => MapDecisionNode(dbo, nodeId, inputDefinitions, outputDefinitions, inputBindings),
            _ => throw new InvalidOperationException($"Unsupported NodeType in database: {dbo.NodeType}")
        };
    }

    private StartWorkflowNode MapStartNode(WorkflowNodeDbo dbo, WorkflowNodeId nodeId, List<NodeInputDefinition> inputDefinitions, List<NodeOutputDefinition> outputDefinitions, List<ParameterBinding> inputBindings)
    {
        var node = new StartWorkflowNode(nodeId, dbo.Name, dbo.BusinessNotes, dbo.IsConfigured, outputDefinitions);
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

        // Deserialize technical inputs dictionary
        var technicalInputs = new Dictionary<string, InputValueSource>();
        if (!string.IsNullOrWhiteSpace(apiDbo.TechnicalInputsJson))
        {
            technicalInputs = JsonSerializer.Deserialize<Dictionary<string, InputValueSource>>(
                apiDbo.TechnicalInputsJson, 
                GetJsonOptions()) ?? new Dictionary<string, InputValueSource>();
        }

        var node = new ApiWorkflowNode(
            id: nodeId,
            name: dbo.Name,
            businessNotes: dbo.BusinessNotes,
            isConfigured: dbo.IsConfigured,
            technicalInputs: technicalInputs,
            retryPolicy: retryPolicy,
            contractMapping: contractMapping,
            inputDefinitions: inputDefinitions,
            outputDefinitions: outputDefinitions
        );

        // Apply deserialized data
        node.UpdateInputBindings(inputBindings);

        return node;
    }

    private NotificationWorkflowNode MapNotificationNode(WorkflowNodeDbo dbo, WorkflowNodeId nodeId, List<NodeInputDefinition> inputDefinitions, List<NodeOutputDefinition> outputDefinitions, List<ParameterBinding> inputBindings)
    {
        if (dbo is not NotificationWorkflowNodeDbo notificationDbo)
            throw new InvalidOperationException($"Expected NotificationWorkflowNodeDbo but got {dbo.GetType().Name}");

        // Deserialize technical inputs dictionary
        var technicalInputs = new Dictionary<string, InputValueSource>();
        if (!string.IsNullOrWhiteSpace(notificationDbo.TechnicalInputsJson))
        {
            technicalInputs = JsonSerializer.Deserialize<Dictionary<string, InputValueSource>>(
                notificationDbo.TechnicalInputsJson, 
                GetJsonOptions()) ?? new Dictionary<string, InputValueSource>();
        }

        var node = new NotificationWorkflowNode(
            id: nodeId,
            name: dbo.Name,
            businessNotes: dbo.BusinessNotes,
            isConfigured: dbo.IsConfigured,
            technicalInputs: technicalInputs,
            inputDefinitions: inputDefinitions,
            outputDefinitions: outputDefinitions
        );

        // Apply deserialized data
        node.UpdateInputBindings(inputBindings);

        return node;
    }

    private HumanTaskWorkflowNode MapHumanTaskNode(WorkflowNodeDbo dbo, WorkflowNodeId nodeId, List<NodeInputDefinition> inputDefinitions, List<NodeOutputDefinition> outputDefinitions, List<ParameterBinding> inputBindings)
    {
        if (dbo is not HumanTaskWorkflowNodeDbo humanTaskDbo)
            throw new InvalidOperationException($"Expected HumanTaskWorkflowNodeDbo but got {dbo.GetType().Name}");

        // Reconstruct value objects
        TaskRole? requiredRole = null;
        if (!string.IsNullOrWhiteSpace(humanTaskDbo.RequiredRole))
        {
            var roleResult = TaskRole.Create(humanTaskDbo.RequiredRole);
            if (roleResult.IsSuccess)
                requiredRole = roleResult.Value;
        }

        TemporalSignalName? signalName = null;
        if (!string.IsNullOrWhiteSpace(humanTaskDbo.SignalName))
        {
            var signalResult = TemporalSignalName.Create(humanTaskDbo.SignalName);
            if (signalResult.IsSuccess)
                signalName = signalResult.Value;
        }

        TaskTimeout? timeout = null;
        if (humanTaskDbo.TimeoutInMinutes.HasValue)
        {
            var timeoutResult = TaskTimeout.Create(humanTaskDbo.TimeoutInMinutes.Value);
            if (timeoutResult.IsSuccess)
                timeout = timeoutResult.Value;
        }

        var node = new HumanTaskWorkflowNode(
            id: nodeId,
            name: dbo.Name,
            businessNotes: dbo.BusinessNotes,
            isConfigured: dbo.IsConfigured,
            requiredRole: requiredRole,
            signalName: signalName,
            timeout: timeout,
            uiFormSchema: humanTaskDbo.UIFormSchema,
            inputDefinitions: inputDefinitions,
            outputDefinitions: outputDefinitions
        );

        // Apply deserialized data
        node.UpdateInputBindings(inputBindings);

        return node;
    }

    private DecisionWorkflowNode MapDecisionNode(WorkflowNodeDbo dbo, WorkflowNodeId nodeId, List<NodeInputDefinition> inputDefinitions, List<NodeOutputDefinition> outputDefinitions, List<ParameterBinding> inputBindings)
    {
        var node = new DecisionWorkflowNode(nodeId, dbo.Name, dbo.BusinessNotes, dbo.IsConfigured);
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
                InputDefinitionsJson = JsonSerializer.Serialize(startNode.InputDefinitions.Select(NodeContractsDtoMapper.ToDto).ToList(), GetJsonOptions()),
                OutputDefinitionsJson = JsonSerializer.Serialize(startNode.OutputDefinitions.Select(NodeContractsDtoMapper.ToDto).ToList(), GetJsonOptions()),
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
                InputDefinitionsJson = JsonSerializer.Serialize(endNode.InputDefinitions.Select(NodeContractsDtoMapper.ToDto).ToList(), GetJsonOptions()),
                OutputDefinitionsJson = JsonSerializer.Serialize(endNode.OutputDefinitions.Select(NodeContractsDtoMapper.ToDto).ToList(), GetJsonOptions()),
                InputBindingsJson = JsonSerializer.Serialize(endNode.InputBindings, GetJsonOptions())
            },
            ApiWorkflowNode apiNode => MapApiNodeToDbo(apiNode, workflowDefinitionId),
            NotificationWorkflowNode notificationNode => MapNotificationNodeToDbo(notificationNode, workflowDefinitionId),
            HumanTaskWorkflowNode humanTaskNode => MapHumanTaskNodeToDbo(humanTaskNode, workflowDefinitionId),
            DecisionWorkflowNode decisionNode => MapDecisionNodeToDbo(decisionNode, workflowDefinitionId),
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
            TechnicalInputsJson = node.TechnicalInputs.Any() ? JsonSerializer.Serialize(node.TechnicalInputs, GetJsonOptions()) : null,
            RetryPolicyMaxAttempts = node.RetryPolicy?.MaxAttempts,
            RetryPolicyBackoffCoefficient = node.RetryPolicy?.BackoffCoefficient,
            ContractMappingConvertXmlToJson = node.ContractMapping?.ConvertXmlToJson,
            ContractMappingQueryParameters = node.ContractMapping?.QueryParameters,
            ContractMappingRequestMapping = node.ContractMapping?.RequestMapping,
            ContractMappingResponseMapping = node.ContractMapping?.ResponseMapping,
            InputDefinitionsJson = JsonSerializer.Serialize(node.InputDefinitions.Select(NodeContractsDtoMapper.ToDto).ToList(), GetJsonOptions()),
            OutputDefinitionsJson = JsonSerializer.Serialize(node.OutputDefinitions.Select(NodeContractsDtoMapper.ToDto).ToList(), GetJsonOptions()),
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
            TechnicalInputsJson = node.TechnicalInputs.Any() ? JsonSerializer.Serialize(node.TechnicalInputs, GetJsonOptions()) : null,
            InputDefinitionsJson = JsonSerializer.Serialize(node.InputDefinitions.Select(NodeContractsDtoMapper.ToDto).ToList(), GetJsonOptions()),
            OutputDefinitionsJson = JsonSerializer.Serialize(node.OutputDefinitions.Select(NodeContractsDtoMapper.ToDto).ToList(), GetJsonOptions()),
            InputBindingsJson = JsonSerializer.Serialize(node.InputBindings, GetJsonOptions())
        };
    }

    private HumanTaskWorkflowNodeDbo MapHumanTaskNodeToDbo(HumanTaskWorkflowNode node, string workflowDefinitionId)
    {
        return new HumanTaskWorkflowNodeDbo
        {
            Id = node.Id.ToString(),
            WorkflowDefinitionId = workflowDefinitionId,
            NodeType = node.Type.Value,
            Name = node.Name,
            BusinessNotes = node.BusinessNotes,
            IsConfigured = node.IsConfigured,
            RequiredRole = node.RequiredRole?.Value,
            SignalName = node.SignalName?.Value,
            TimeoutInMinutes = node.Timeout?.TimeoutInMinutes,
            UIFormSchema = node.UIFormSchema,
            InputDefinitionsJson = JsonSerializer.Serialize(node.InputDefinitions.Select(NodeContractsDtoMapper.ToDto).ToList(), GetJsonOptions()),
            OutputDefinitionsJson = JsonSerializer.Serialize(node.OutputDefinitions.Select(NodeContractsDtoMapper.ToDto).ToList(), GetJsonOptions()),
            InputBindingsJson = JsonSerializer.Serialize(node.InputBindings, GetJsonOptions())
        };
    }

    private DecisionWorkflowNodeDbo MapDecisionNodeToDbo(DecisionWorkflowNode node, string workflowDefinitionId)
    {
        return new DecisionWorkflowNodeDbo
        {
            Id = node.Id.ToString(),
            WorkflowDefinitionId = workflowDefinitionId,
            NodeType = node.Type.Value,
            Name = node.Name,
            BusinessNotes = node.BusinessNotes,
            IsConfigured = node.IsConfigured,
            InputDefinitionsJson = JsonSerializer.Serialize(node.InputDefinitions.Select(NodeContractsDtoMapper.ToDto).ToList(), GetJsonOptions()),
            OutputDefinitionsJson = JsonSerializer.Serialize(node.OutputDefinitions.Select(NodeContractsDtoMapper.ToDto).ToList(), GetJsonOptions()),
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
