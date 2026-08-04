using Microsoft.EntityFrameworkCore;
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

        return MapToDomain(dbo, nodeDbos);
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

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private WorkflowDefinition MapToDomain(WorkflowDefinitionDbo dbo, List<WorkflowNodeDbo> nodeDbos)
    {
        var id = WorkflowDefinitionId.Create(dbo.Id).Value ?? throw new InvalidOperationException($"Invalid WorkflowDefinitionId in database: {dbo.Id}");
        var publicId = WorkflowDefinitionPublicId.Create(dbo.PublicId).Value ?? throw new InvalidOperationException($"Invalid WorkflowDefinitionPublicId in database: {dbo.PublicId}");
        var creatorId = UserId.Create(dbo.CreatorId).Value ?? throw new InvalidOperationException($"Invalid UserId in database: {dbo.CreatorId}");
        
        var statusResult = WorkflowStatus.FromValue(dbo.Status);
        if (statusResult.IsFailure)
            throw new InvalidOperationException($"Invalid WorkflowStatus in database: {dbo.Status}. {statusResult.Error}");
        var status = statusResult.Value;

        var nodes = nodeDbos.Select(MapDboToNode).ToList();

        // Use internal constructor for rehydration
        return new WorkflowDefinition(
            id: id,
            publicId: publicId,
            creatorId: creatorId,
            name: dbo.Name,
            status: status,
            flowJson: dbo.FlowJson,
            nodes: nodes
        );
    }

    private WorkflowNode MapDboToNode(WorkflowNodeDbo dbo)
    {
        var nodeId = WorkflowNodeId.Create(dbo.Id).Value ?? throw new InvalidOperationException($"Invalid WorkflowNodeId in database: {dbo.Id}");
        
        var nodeTypeResult = NodeType.FromValue(dbo.NodeType);
        if (nodeTypeResult.IsFailure)
            throw new InvalidOperationException($"Invalid NodeType in database: {dbo.NodeType}. {nodeTypeResult.Error}");
        var nodeType = nodeTypeResult.Value;

        return nodeType switch
        {
            var t when t == NodeType.Api => MapApiNode(dbo, nodeId),
            var t when t == NodeType.Notification => MapNotificationNode(dbo, nodeId),
            _ => throw new InvalidOperationException($"Unsupported NodeType in database: {dbo.NodeType}")
        };
    }

    private ApiWorkflowNode MapApiNode(WorkflowNodeDbo dbo, WorkflowNodeId nodeId)
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

        return new ApiWorkflowNode(
            id: nodeId,
            name: dbo.Name,
            businessNotes: dbo.BusinessNotes,
            isConfigured: dbo.IsConfigured,
            endpointUrl: apiDbo.EndpointUrl,
            authToken: apiDbo.AuthToken,
            retryPolicy: retryPolicy,
            contractMapping: contractMapping
        );
    }

    private NotificationWorkflowNode MapNotificationNode(WorkflowNodeDbo dbo, WorkflowNodeId nodeId)
    {
        if (dbo is not NotificationWorkflowNodeDbo notificationDbo)
            throw new InvalidOperationException($"Expected NotificationWorkflowNodeDbo but got {dbo.GetType().Name}");

        return new NotificationWorkflowNode(
            id: nodeId,
            name: dbo.Name,
            businessNotes: dbo.BusinessNotes,
            isConfigured: dbo.IsConfigured,
            messageTemplate: notificationDbo.MessageTemplate
        );
    }

    private void MapToDbo(WorkflowDefinition workflow, WorkflowDefinitionDbo dbo)
    {
        dbo.Id = workflow.Id.ToString();
        dbo.PublicId = workflow.PublicId.ToString();
        dbo.CreatorId = workflow.CreatorId.ToString();
        dbo.Name = workflow.Name;
        dbo.Status = workflow.Status.Value;
        dbo.FlowJson = workflow.FlowJson;
    }

    private WorkflowNodeDbo MapNodeToDbo(WorkflowNode node, string workflowDefinitionId)
    {
        return node switch
        {
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
            ContractMappingResponseMapping = node.ContractMapping?.ResponseMapping
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
            MessageTemplate = node.MessageTemplate
        };
    }
}
