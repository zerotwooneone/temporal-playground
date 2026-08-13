using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration.Nodes;
using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

namespace TemporalDDD.Application.WorkflowOrchestration;

public class WorkflowNodeService : IWorkflowNodeService
{
    private readonly IWorkflowDefinitionRepository _workflowDefinitionRepository;

    public WorkflowNodeService(IWorkflowDefinitionRepository workflowDefinitionRepository)
    {
        _workflowDefinitionRepository = workflowDefinitionRepository;
    }

    public async Task UpdateNodesAsync(WorkflowDefinitionId workflowDefinitionId, string flowJson, UpdateWorkflowNodesInput input, CancellationToken cancellationToken = default)
    {
        // Validate workflow exists
        var workflow = await _workflowDefinitionRepository.GetByIdAsync(workflowDefinitionId, cancellationToken);
        if (workflow == null)
            throw new InvalidOperationException($"Workflow with WorkflowDefinitionId {workflowDefinitionId} not found");

        // Map DTOs to domain nodes
        var domainNodes = new List<WorkflowNode>();
        foreach (var nodeDto in input.Nodes)
        {
            var nodeTypeResult = NodeType.FromValue(nodeDto.NodeType);
            if (nodeTypeResult.IsFailure)
                throw new InvalidOperationException($"Invalid NodeType for node {nodeDto.Id}: {nodeTypeResult.Error}");

            var nodeIdResult = WorkflowNodeId.Create(nodeDto.Id);
            if (nodeIdResult.IsFailure)
                throw new InvalidOperationException($"Invalid NodeId for node {nodeDto.Id}: {nodeIdResult.Error}");

            WorkflowNode domainNode = nodeTypeResult.Value.Value switch
            {
                0 => StartWorkflowNode.CreateStub(nodeDto.Name, nodeDto.BusinessNotes),
                1 => ApiWorkflowNode.CreateStub(nodeDto.Name, nodeDto.BusinessNotes),
                2 => NotificationWorkflowNode.CreateStub(nodeDto.Name, nodeDto.BusinessNotes),
                3 => DecisionWorkflowNode.CreateStub(nodeDto.Name, nodeDto.BusinessNotes),
                4 => HumanTaskWorkflowNode.CreateStub(nodeDto.Name, nodeDto.BusinessNotes),
                99 => EndWorkflowNode.CreateStub(nodeDto.Name, nodeDto.BusinessNotes),
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

                apiNode.SetTechnicalInput(ApiWorkflowNode.EndpointUrlKey, new InputValueSource.Fixed(nodeDto.EndpointUrl ?? string.Empty));
                if (nodeDto.AuthToken != null)
                {
                    apiNode.SetTechnicalInput(ApiWorkflowNode.AuthTokenKey, new InputValueSource.Fixed(nodeDto.AuthToken));
                }
                apiNode.ConfigureValueObjects(retryPolicy, contractMapping);
            }

            // Configure notification node specifics
            if (nodeTypeResult.Value.Value == 2 && domainNode is NotificationWorkflowNode notificationNode)
            {
                notificationNode.SetTechnicalInput(NotificationWorkflowNode.MessageTemplateKey, new InputValueSource.Fixed(nodeDto.MessageTemplate ?? string.Empty));
            }

            domainNodes.Add(domainNode);
        }

        // Map transition DTOs to domain transitions
        var domainTransitions = new List<WorkflowTransition>();
        foreach (var transitionDto in input.Transitions)
        {
            var sourceNodeIdResult = WorkflowNodeId.Create(transitionDto.SourceNodeId);
            if (sourceNodeIdResult.IsFailure)
                throw new InvalidOperationException($"Invalid SourceNodeId: {sourceNodeIdResult.Error}");

            var targetNodeIdResult = WorkflowNodeId.Create(transitionDto.TargetNodeId);
            if (targetNodeIdResult.IsFailure)
                throw new InvalidOperationException($"Invalid TargetNodeId: {targetNodeIdResult.Error}");

            domainTransitions.Add(new WorkflowTransition(sourceNodeIdResult.Value, targetNodeIdResult.Value));
        }

        // Update workflow with new nodes and transitions
        workflow.UpdateNodes(domainNodes, domainTransitions, flowJson);
        await _workflowDefinitionRepository.SaveAsync(workflow, cancellationToken);
    }
}
