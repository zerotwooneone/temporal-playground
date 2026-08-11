using Microsoft.Extensions.DependencyInjection;
using TemporalDDD.Application.Messaging;
using TemporalDDD.Application.WorkflowOrchestration;
using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration.Nodes;
using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;
using Temporalio.Activities;

namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public class WorkflowOrchestrationActivities : IWorkflowOrchestrationActivities
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IWorkflowEventMapper _eventMapper;
    private readonly IMessagePublisher _messagePublisher;

    public WorkflowOrchestrationActivities(
        IServiceScopeFactory scopeFactory,
        IWorkflowEventMapper eventMapper,
        IMessagePublisher messagePublisher)
    {
        _scopeFactory = scopeFactory;
        _eventMapper = eventMapper;
        _messagePublisher = messagePublisher;
    }

    [Activity]
    public async Task<SaveWorkflowResult> UpdateNodesAndSaveAsync(UpdateWorkflowNodesInput input)
    {
        using var scope = _scopeFactory.CreateScope();
        var workflowDefinitionRepository = scope.ServiceProvider.GetRequiredService<IWorkflowDefinitionRepository>();

        // Validate workflow exists
        var workflowIdResult = WorkflowDefinitionId.Create(input.WorkflowDefinitionId);
        if (workflowIdResult.IsFailure)
            throw new InvalidOperationException($"Invalid WorkflowDefinitionId: {workflowIdResult.Error}");

        var workflow = await workflowDefinitionRepository.GetByIdAsync(workflowIdResult.Value);
        if (workflow == null)
            throw new InvalidOperationException($"Workflow with WorkflowDefinitionId {input.WorkflowDefinitionId} not found");

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
        workflow.UpdateNodes(domainNodes, domainTransitions, null);
        await workflowDefinitionRepository.SaveAsync(workflow);

        // Map domain events to application events
        var domainEvents = workflow.DomainEvents;
        var applicationEvents = domainEvents
            .Select(e => _eventMapper.MapToApplicationEvent(e))
            .ToList();

        // Return result with events
        return new SaveWorkflowResult(
            WorkflowId: workflow.Id.ToString(),
            Events: applicationEvents
        );
    }

    [Activity]
    public async Task PublishApplicationEventsAsync(PublishEventsInput input)
    {
        foreach (var applicationEvent in input.Events)
        {
            await _messagePublisher.PublishEventAsync(applicationEvent);
        }
    }
}
