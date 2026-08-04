using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration.Events;
using TemporalDDD.Domain.SeedWork;

namespace TemporalDDD.Domain.WorkflowOrchestration;

public sealed class WorkflowDefinition : AggregateRoot
{
    public WorkflowDefinitionId Id { get; private set; }
    public WorkflowDefinitionPublicId PublicId { get; private set; }
    public UserId CreatorId { get; private set; }
    public string Name { get; private set; }
    public WorkflowStatus Status { get; private set; }
    public string FlowJson { get; private set; }

    private WorkflowDefinition() { }

    // Internal constructor for infrastructure rehydration
    internal WorkflowDefinition(
        WorkflowDefinitionId id,
        WorkflowDefinitionPublicId publicId,
        UserId creatorId,
        string name,
        WorkflowStatus status,
        string flowJson)
    {
        Id = id;
        PublicId = publicId;
        CreatorId = creatorId;
        Name = name;
        Status = status;
        FlowJson = flowJson;
    }

    // Factory for creating new workflow definition
    public static WorkflowDefinition Create(UserId creatorId, string name, string initialJson)
    {
        var workflow = new WorkflowDefinition
        {
            Id = WorkflowDefinitionId.New(),
            PublicId = WorkflowDefinitionPublicId.New(),
            CreatorId = creatorId,
            Name = name,
            Status = WorkflowStatus.Draft,
            FlowJson = initialJson
        };

        workflow.RaiseDomainEvent(new WorkflowDraftCreated(
            workflow.Id,
            workflow.PublicId,
            workflow.CreatorId,
            workflow.Name));

        return workflow;
    }

    public void UpdateFlowJson(string flowJson)
    {
        if (Status != WorkflowStatus.Draft && Status != WorkflowStatus.Rejected)
            throw new InvalidOperationException("Cannot update flow JSON when status is not Draft or Rejected");

        FlowJson = flowJson;
    }

    public void SubmitForReview()
    {
        if (Status != WorkflowStatus.Draft && Status != WorkflowStatus.Rejected)
            throw new InvalidOperationException("Cannot submit for review when status is not Draft or Rejected");

        Status = WorkflowStatus.PendingReview;
        RaiseDomainEvent(new WorkflowSubmittedForReview(Id));
    }

    public void Approve(UserId reviewerId)
    {
        if (Status != WorkflowStatus.PendingReview)
            throw new InvalidOperationException("Cannot approve workflow when status is not PendingReview");

        Status = WorkflowStatus.Approved;
        RaiseDomainEvent(new WorkflowApproved(Id, reviewerId));
    }

    public void Reject(UserId reviewerId, string reason)
    {
        if (Status != WorkflowStatus.PendingReview)
            throw new InvalidOperationException("Cannot reject workflow when status is not PendingReview");

        Status = WorkflowStatus.Rejected;
        RaiseDomainEvent(new WorkflowRejected(Id, reviewerId, reason));
    }
}
