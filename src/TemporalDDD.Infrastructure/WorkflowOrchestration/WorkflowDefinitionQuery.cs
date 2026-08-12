using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TemporalDDD.Application.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration.Nodes;
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
                var technicalInputs = DeserializeTechnicalInputs(apiNode.TechnicalInputsJson);
                return new WorkflowNodeDetailDto(
                    Id: n.Id,
                    NodeType: n.NodeType,
                    Name: n.Name,
                    BusinessNotes: n.BusinessNotes,
                    IsConfigured: n.IsConfigured,
                    EndpointUrl: ExtractFixedValueFromDictionary(technicalInputs, "EndpointUrl"),
                    AuthToken: ExtractFixedValueFromDictionary(technicalInputs, "AuthToken"),
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
                var technicalInputs = DeserializeTechnicalInputs(notificationNode.TechnicalInputsJson);
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
                    MessageTemplate: ExtractFixedValueFromDictionary(technicalInputs, "MessageTemplate")
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

    public async Task<WorkflowDefinitionId?> GetWorkflowDefinitionIdByPublicIdAsync(WorkflowDefinitionPublicId publicId, CancellationToken cancellationToken = default)
    {
        var workflow = await _dbContext.WorkflowDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.PublicId == publicId.ToString(), cancellationToken);

        if (workflow == null)
            return null;

        var idResult = WorkflowDefinitionId.Create(workflow.Id);
        if (idResult.IsFailure)
            return null;

        return idResult.Value;
    }

    private static Dictionary<string, InputValueSource> DeserializeTechnicalInputs(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, InputValueSource>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, InputValueSource>>(
                json, 
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) 
                ?? new Dictionary<string, InputValueSource>();
        }
        catch
        {
            return new Dictionary<string, InputValueSource>();
        }
    }

    private static string? ExtractFixedValueFromDictionary(Dictionary<string, InputValueSource> technicalInputs, string key)
    {
        if (!technicalInputs.TryGetValue(key, out var valueSource))
            return null;

        if (valueSource is InputValueSource.Fixed fixedValue)
            return fixedValue.Value;

        return null;
    }
}
