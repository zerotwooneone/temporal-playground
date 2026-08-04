using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration.Events;
using TemporalDDD.Domain.WorkflowOrchestration.Nodes;
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

    private readonly List<WorkflowNode> _nodes = new();
    public IReadOnlyCollection<WorkflowNode> Nodes => _nodes.AsReadOnly();

    private WorkflowDefinition() { }

    // Internal constructor for infrastructure rehydration
    internal WorkflowDefinition(
        WorkflowDefinitionId id,
        WorkflowDefinitionPublicId publicId,
        UserId creatorId,
        string name,
        WorkflowStatus status,
        string flowJson,
        IEnumerable<WorkflowNode> nodes)
    {
        Id = id;
        PublicId = publicId;
        CreatorId = creatorId;
        Name = name;
        Status = status;
        FlowJson = flowJson;
        _nodes.AddRange(nodes);
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

        if (_nodes.Any(n => !n.IsConfigured))
            throw new InvalidOperationException("Cannot approve workflow: One or more nodes are missing technical configuration.");

        Status = WorkflowStatus.Approved;
        RaiseDomainEvent(new WorkflowApproved(Id, reviewerId));
    }

    public void AddApiNodeStub(string name, string? businessNotes)
    {
        _nodes.Add(ApiWorkflowNode.CreateStub(name, businessNotes));
    }

    public void AddNotificationNodeStub(string name, string? businessNotes)
    {
        _nodes.Add(NotificationWorkflowNode.CreateStub(name, businessNotes));
    }

    public void Reject(UserId reviewerId, string reason)
    {
        if (Status != WorkflowStatus.PendingReview)
            throw new InvalidOperationException("Cannot reject workflow when status is not PendingReview");

        Status = WorkflowStatus.Rejected;
        RaiseDomainEvent(new WorkflowRejected(Id, reviewerId, reason));
    }
}
