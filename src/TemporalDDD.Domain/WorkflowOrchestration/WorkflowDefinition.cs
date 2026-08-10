using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration.Events;
using TemporalDDD.Domain.WorkflowOrchestration.Nodes;
using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;
using TemporalDDD.Domain.SeedWork;
using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.WorkflowOrchestration;

public sealed class WorkflowDefinition : AggregateRoot
{
    public WorkflowDefinitionId Id { get; private set; }
    public WorkflowDefinitionPublicId PublicId { get; private set; }
    public UserId CreatorId { get; private set; }
    public string Name { get; private set; }
    public WorkflowClassName ClassName { get; private set; }
    public WorkflowStatus Status { get; private set; }
    public string FlowJson { get; private set; }

    private readonly List<WorkflowNode> _nodes = new();
    public IReadOnlyCollection<WorkflowNode> Nodes => _nodes.AsReadOnly();

    private readonly List<WorkflowTransition> _transitions = new();
    public IReadOnlyList<WorkflowTransition> Transitions => _transitions.AsReadOnly();

    private WorkflowDefinition() { }

    // Internal constructor for infrastructure rehydration
    internal WorkflowDefinition(
        WorkflowDefinitionId id,
        WorkflowDefinitionPublicId publicId,
        UserId creatorId,
        string name,
        WorkflowClassName className,
        WorkflowStatus status,
        string flowJson,
        IEnumerable<WorkflowNode> nodes)
    {
        Id = id;
        PublicId = publicId;
        CreatorId = creatorId;
        Name = name;
        ClassName = className;
        Status = status;
        FlowJson = flowJson;
        _nodes.AddRange(nodes);
    }

    // Factory for creating new workflow definition
    public static WorkflowDefinition Create(UserId creatorId, string name, string initialJson)
    {
        var publicId = WorkflowDefinitionPublicId.New();
        var classNameResult = WorkflowClassName.Create(name, publicId);

        if (classNameResult.IsFailure)
            throw new ArgumentException(classNameResult.Error);

        var workflow = new WorkflowDefinition
        {
            Id = WorkflowDefinitionId.New(),
            PublicId = publicId,
            CreatorId = creatorId,
            Name = name,
            ClassName = classNameResult.Value,
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

    public void UpdateName(string newName)
    {
        if (Status != WorkflowStatus.Draft && Status != WorkflowStatus.Rejected)
            throw new InvalidOperationException("Cannot update name when status is not Draft or Rejected");

        var classNameResult = WorkflowClassName.Create(newName, PublicId);
        if (classNameResult.IsFailure)
            throw new ArgumentException(classNameResult.Error);

        Name = newName;
        ClassName = classNameResult.Value;
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

        var topologyValidation = ValidateTopology();
        if (topologyValidation.IsFailure)
            throw new InvalidOperationException($"Cannot approve workflow: {topologyValidation.Error}");

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

    public void UpdateNodes(IReadOnlyList<WorkflowNode> nodes, IReadOnlyList<WorkflowTransition> transitions, string? flowJson)
    {
        _nodes.Clear();
        _nodes.AddRange(nodes);
        _transitions.Clear();
        _transitions.AddRange(transitions);
        if (flowJson != null)
        {
            FlowJson = flowJson;
        }
        RaiseDomainEvent(new WorkflowNodesUpdated(Id));
    }

    public Result ValidateTopology()
    {
        // Rule 1: Exactly one Start node
        var startNodes = _nodes.Where(n => n.Type == NodeType.Start).ToList();
        if (startNodes.Count != 1)
        {
            return Result.Failure($"Workflow must have exactly one Start node. Found: {startNodes.Count}");
        }

        // Rule 2: At least one End node
        var endNodes = _nodes.Where(n => n.Type == NodeType.End).ToList();
        if (endNodes.Count == 0)
        {
            return Result.Failure("Workflow must have at least one End node");
        }

        // Build adjacency list for graph traversal
        var adjacency = _transitions
            .GroupBy(t => t.SourceNodeId)
            .ToDictionary(g => g.Key, g => g.Select(t => t.TargetNodeId).ToList());

        var nodeIds = _nodes.Select(n => n.Id).ToHashSet();

        // Rule 3: Every node must be reachable from Start
        var startNodeId = startNodes[0].Id;
        var reachableFromStart = new HashSet<WorkflowNodeId>();
        var queue = new Queue<WorkflowNodeId>();
        queue.Enqueue(startNodeId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (reachableFromStart.Contains(current))
                continue;

            reachableFromStart.Add(current);

            if (adjacency.TryGetValue(current, out var neighbors))
            {
                foreach (var neighbor in neighbors)
                {
                    if (!reachableFromStart.Contains(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        var unreachableNodes = _nodes.Where(n => !reachableFromStart.Contains(n.Id)).ToList();
        if (unreachableNodes.Any())
        {
            return Result.Failure($"The following nodes are not reachable from Start: {string.Join(", ", unreachableNodes.Select(n => n.Name))}");
        }

        // Rule 4: Valid path from Start to at least one End node
        var endNodeIds = endNodes.Select(n => n.Id).ToHashSet();
        var hasPathToEnd = reachableFromStart.Any(id => endNodeIds.Contains(id));

        if (!hasPathToEnd)
        {
            return Result.Failure("No valid path exists from Start node to any End node");
        }

        // Rule 5: Detect cycles (ensure DAG)
        var visited = new HashSet<WorkflowNodeId>();
        var recursionStack = new HashSet<WorkflowNodeId>();

        bool HasCycle(WorkflowNodeId nodeId)
        {
            visited.Add(nodeId);
            recursionStack.Add(nodeId);

            if (adjacency.TryGetValue(nodeId, out var neighbors))
            {
                foreach (var neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor))
                    {
                        if (HasCycle(neighbor))
                            return true;
                    }
                    else if (recursionStack.Contains(neighbor))
                    {
                        return true;
                    }
                }
            }

            recursionStack.Remove(nodeId);
            return false;
        }

        foreach (var node in _nodes)
        {
            var nodeId = node.Id;
            if (!visited.Contains(nodeId))
            {
                if (HasCycle(nodeId))
                {
                    return Result.Failure("Workflow contains a cycle. Workflows must be acyclic (DAG).");
                }
            }
        }

        return Result.Success();
    }

    public void Reject(UserId reviewerId, string reason)
    {
        if (Status != WorkflowStatus.PendingReview)
            throw new InvalidOperationException("Cannot reject workflow when status is not PendingReview");

        Status = WorkflowStatus.Rejected;
        RaiseDomainEvent(new WorkflowRejected(Id, reviewerId, reason));
    }

    public void Publish()
    {
        if (Status != WorkflowStatus.Approved)
            throw new InvalidOperationException("Cannot publish workflow when status is not Approved");

        Status = WorkflowStatus.Published;
    }
}
