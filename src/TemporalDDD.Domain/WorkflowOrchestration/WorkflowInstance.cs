using TemporalDDD.Domain.WorkflowOrchestration.Events;
using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;
using TemporalDDD.Domain.SeedWork;

namespace TemporalDDD.Domain.WorkflowOrchestration;

public sealed class WorkflowInstance : AggregateRoot
{
    public WorkflowInstanceId Id { get; private set; }
    public WorkflowInstancePublicId PublicId { get; private set; }
    public WorkflowDefinitionId WorkflowDefinitionId { get; private set; }
    public BusinessReferenceId BusinessReferenceId { get; private set; }
    public string TemporalRunId { get; private set; }
    public WorkflowInstanceStatus Status { get; private set; }
    public WorkflowContextData ContextData { get; private set; }

    private WorkflowInstance() { }

    // Internal constructor for infrastructure rehydration
    internal WorkflowInstance(
        WorkflowInstanceId id,
        WorkflowInstancePublicId publicId,
        WorkflowDefinitionId workflowDefinitionId,
        BusinessReferenceId businessReferenceId,
        string temporalRunId,
        WorkflowInstanceStatus status,
        WorkflowContextData contextData)
    {
        Id = id;
        PublicId = publicId;
        WorkflowDefinitionId = workflowDefinitionId;
        BusinessReferenceId = businessReferenceId;
        TemporalRunId = temporalRunId;
        Status = status;
        ContextData = contextData;
    }

    // Factory method to start a new workflow instance
    public static WorkflowInstance StartNew(WorkflowDefinitionId workflowDefinitionId, BusinessReferenceId businessReferenceId, string temporalRunId)
    {
        var instance = new WorkflowInstance
        {
            Id = WorkflowInstanceId.New(),
            PublicId = WorkflowInstancePublicId.New(),
            WorkflowDefinitionId = workflowDefinitionId,
            BusinessReferenceId = businessReferenceId,
            TemporalRunId = temporalRunId,
            Status = WorkflowInstanceStatus.Running,
            ContextData = WorkflowContextData.Empty()
        };

        instance.RaiseDomainEvent(new WorkflowInstanceStarted(instance.PublicId, instance.BusinessReferenceId));

        return instance;
    }

    public void Complete()
    {
        if (Status != WorkflowInstanceStatus.Running && Status != WorkflowInstanceStatus.Suspended)
            throw new InvalidOperationException("Cannot complete workflow instance when not in Running or Suspended state");

        Status = WorkflowInstanceStatus.Completed;
    }

    public void Fail(string reason)
    {
        if (Status != WorkflowInstanceStatus.Running && Status != WorkflowInstanceStatus.Suspended)
            throw new InvalidOperationException("Cannot fail workflow instance when not in Running or Suspended state");

        Status = WorkflowInstanceStatus.Failed;
    }

    public void Suspend()
    {
        if (Status != WorkflowInstanceStatus.Running)
            throw new InvalidOperationException("Cannot suspend workflow instance when not in Running state");

        Status = WorkflowInstanceStatus.Suspended;
    }

    public void Resume()
    {
        if (Status != WorkflowInstanceStatus.Suspended)
            throw new InvalidOperationException("Cannot resume workflow instance when not in Suspended state");

        Status = WorkflowInstanceStatus.Running;
    }

    public void UpdateContext(WorkflowContextData newData)
    {
        ContextData = newData;
    }
}
