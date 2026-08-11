using Microsoft.EntityFrameworkCore;
using TemporalDDD.Application.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public class WorkflowDefinitionQuery : IWorkflowDefinitionQuery
{
    private readonly ApplicationDbContext _dbContext;

    public WorkflowDefinitionQuery(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<WorkflowDefinitionDto>> GetAllWorkflowsAsync(CancellationToken cancellationToken = default)
    {
        var workflows = await _dbContext.WorkflowDefinitions
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var result = new List<WorkflowDefinitionDto>();

        foreach (var dbo in workflows)
        {
            var statusResult = WorkflowStatus.FromValue(dbo.Status);
            if (statusResult.IsFailure)
                continue; // Skip invalid entries

            var nodeCount = await _dbContext.WorkflowNodes
                .AsNoTracking()
                .CountAsync(n => n.WorkflowDefinitionId == dbo.Id, cancellationToken);

            result.Add(new WorkflowDefinitionDto(
                PublicId: dbo.PublicId,
                Name: dbo.Name,
                Status: statusResult.Value.Name,
                NodeCount: nodeCount
            ));
        }

        return result.AsReadOnly();
    }

    public async Task<WorkflowDetailDto?> GetWorkflowByPublicIdAsync(WorkflowDefinitionPublicId publicId, CancellationToken cancellationToken = default)
    {
        var workflow = await _dbContext.WorkflowDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.PublicId == publicId.ToString(), cancellationToken);

        if (workflow == null)
            return null;

        var statusResult = WorkflowStatus.FromValue(workflow.Status);
        if (statusResult.IsFailure)
            return null;

        var nodes = await _dbContext.WorkflowNodes
            .AsNoTracking()
            .Where(n => n.WorkflowDefinitionId == workflow.Id)
            .ToListAsync(cancellationToken);

        var nodeDtos = nodes.Select(n =>
        {
            // Handle polymorphic node types
            if (n is ApiWorkflowNodeDbo apiNode)
            {
                return new WorkflowNodeDetailDto(
                    Id: n.Id,
                    NodeType: n.NodeType,
                    Name: n.Name,
                    BusinessNotes: n.BusinessNotes,
                    IsConfigured: n.IsConfigured,
                    EndpointUrl: apiNode.EndpointUrl,
                    AuthToken: apiNode.AuthToken,
                    RetryPolicyMaxAttempts: apiNode.RetryPolicyMaxAttempts,
                    RetryPolicyBackoffCoefficient: apiNode.RetryPolicyBackoffCoefficient,
                    ContractMappingConvertXmlToJson: apiNode.ContractMappingConvertXmlToJson,
                    ContractMappingQueryParameters: apiNode.ContractMappingQueryParameters,
                    ContractMappingRequestMapping: apiNode.ContractMappingRequestMapping,
                    ContractMappingResponseMapping: apiNode.ContractMappingResponseMapping,
                    MessageTemplate: null
                );
            }
            else if (n is NotificationWorkflowNodeDbo notificationNode)
            {
                return new WorkflowNodeDetailDto(
                    Id: n.Id,
                    NodeType: n.NodeType,
                    Name: n.Name,
                    BusinessNotes: n.BusinessNotes,
                    IsConfigured: n.IsConfigured,
                    EndpointUrl: null,
                    AuthToken: null,
                    RetryPolicyMaxAttempts: null,
                    RetryPolicyBackoffCoefficient: null,
                    ContractMappingConvertXmlToJson: null,
                    ContractMappingQueryParameters: null,
                    ContractMappingRequestMapping: null,
                    ContractMappingResponseMapping: null,
                    MessageTemplate: notificationNode.MessageTemplate
                );
            }
            else
            {
                // Base WorkflowNodeDbo (shouldn't happen in practice)
                return new WorkflowNodeDetailDto(
                    Id: n.Id,
                    NodeType: n.NodeType,
                    Name: n.Name,
                    BusinessNotes: n.BusinessNotes,
                    IsConfigured: n.IsConfigured,
                    EndpointUrl: null,
                    AuthToken: null,
                    RetryPolicyMaxAttempts: null,
                    RetryPolicyBackoffCoefficient: null,
                    ContractMappingConvertXmlToJson: null,
                    ContractMappingQueryParameters: null,
                    ContractMappingRequestMapping: null,
                    ContractMappingResponseMapping: null,
                    MessageTemplate: null
                );
            }
        }).ToList();

        return new WorkflowDetailDto(
            PublicId: workflow.PublicId,
            Name: workflow.Name,
            Status: statusResult.Value.Name,
            FlowJson: workflow.FlowJson ?? string.Empty,
            Nodes: nodeDtos
        );
    }
}
