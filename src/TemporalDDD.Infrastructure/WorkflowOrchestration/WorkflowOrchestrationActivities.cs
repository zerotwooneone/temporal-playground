using Microsoft.Extensions.DependencyInjection;
using TemporalDDD.Application.Messaging;
using TemporalDDD.Application.WorkflowOrchestration;
using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration;
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
    public async Task<SaveWorkflowResult> CreateDraftAndSaveAsync(CreateWorkflowDraftInput input)
    {
        using var scope = _scopeFactory.CreateScope();
        var workflowDefinitionRepository = scope.ServiceProvider.GetRequiredService<IWorkflowDefinitionRepository>();

        // Convert primitive DTO to Domain types with fail-fast validation
        var creatorIdResult = UserId.Create(input.CreatorId);
        if (creatorIdResult.IsFailure)
            throw new InvalidOperationException($"Internal Corruption: Invalid UserId. {creatorIdResult.Error}");

        var creatorId = creatorIdResult.Value;

        // Create domain entity using factory
        var workflow = WorkflowDefinition.Create(
            creatorId: creatorId,
            name: input.Name,
            initialJson: "{}");

        // Save to database using repository
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
