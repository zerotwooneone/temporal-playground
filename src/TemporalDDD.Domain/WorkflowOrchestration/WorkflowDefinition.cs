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

    private readonly List<NodeOutputDefinition> _expectedInputs = new();
    public IReadOnlyList<NodeOutputDefinition> ExpectedInputs => _expectedInputs.AsReadOnly();

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
        IEnumerable<WorkflowNode> nodes,
        IEnumerable<WorkflowTransition> transitions)
    {
        Id = id;
        PublicId = publicId;
        CreatorId = creatorId;
        Name = name;
        ClassName = className;
        Status = status;
        FlowJson = flowJson;
        _nodes.AddRange(nodes);
        _transitions.AddRange(transitions);
    }

    // Factory for creating new workflow definition
    public static WorkflowDefinition Create(UserId creatorId, string name, string initialJson, WorkflowDefinitionPublicId publicId)
    {
        var classNameResult = WorkflowClassName.Create(name, publicId);

        if (classNameResult.IsFailure)
            throw new ArgumentException(classNameResult.Error);

        var startNode = StartWorkflowNode.CreateStub("Start", "Entry point of the workflow");
        var endNode = EndWorkflowNode.CreateStub("End", "Successful completion");

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

        workflow._nodes.Add(startNode);
        workflow._nodes.Add(endNode);

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

        var executionScopeValidation = ValidateExecutionScopes();
        if (executionScopeValidation.IsFailure)
            throw new InvalidOperationException($"Cannot approve workflow: {executionScopeValidation.Error}");

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
    }

    public void UpdateWorkflowInputs(IEnumerable<NodeOutputDefinition> inputs)
    {
        _expectedInputs.Clear();
        _expectedInputs.AddRange(inputs);

        // Sync the workflow inputs to the Start Node's outputs so downstream nodes can map to them
        var startNode = _nodes.OfType<StartWorkflowNode>().Single();
        startNode.ConfigureOutputs(_expectedInputs);
    }

    private WorkflowNode GetNode(WorkflowNodeId nodeId)
    {
        return _nodes.FirstOrDefault(n => n.Id == nodeId)
            ?? throw new InvalidOperationException($"Node with ID '{nodeId}' not found in workflow.");
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

        // Rule 6: Decision nodes must have at least one branch with explicit labels
        var decisionNodes = _nodes.OfType<DecisionWorkflowNode>();
        foreach (var decisionNode in decisionNodes)
        {
            var outgoingTransitions = _transitions.Where(t => t.SourceNodeId == decisionNode.Id).ToList();
            
            // Rule 1: Must go somewhere
            if (!outgoingTransitions.Any())
            {
                return Result.Failure($"Decision Node '{decisionNode.Name}' must have at least one outgoing transition.");
            }

            // Rule 2: The branches it does have must be explicitly labeled
            if (outgoingTransitions.Any(t => string.IsNullOrWhiteSpace(t.BranchLabel)))
            {
                return Result.Failure($"All outgoing transitions from Decision Node '{decisionNode.Name}' must have a BranchLabel (e.g., 'True' or 'False').");
            }
        }

        return Result.Success();
    }

    private HashSet<WorkflowNodeId> GetExecutionScopeForNode(WorkflowNodeId targetNodeId)
    {
        var scope = new HashSet<WorkflowNodeId>();
        var queue = new Queue<WorkflowNodeId>();

        // Seed the queue with the immediate parents of the target node
        var immediateParents = _transitions
            .Where(t => t.TargetNodeId == targetNodeId)
            .Select(t => t.SourceNodeId);

        foreach (var parent in immediateParents)
        {
            queue.Enqueue(parent);
        }

        // Traverse backward up the DAG
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            // HashSet.Add returns false if the item was already present, protecting us from infinite loops 
            // in case topology validation hasn't caught a cycle yet.
            if (scope.Add(current))
            {
                var nextParents = _transitions
                    .Where(t => t.TargetNodeId == current)
                    .Select(t => t.SourceNodeId);

                foreach (var nextParent in nextParents)
                {
                    queue.Enqueue(nextParent);
                }
            }
        }

        return scope;
    }

    private Result ValidateExecutionScopes()
    {
        foreach (var node in _nodes.Where(n => n.InputBindings.Any()))
        {
            // Get all guaranteed ancestors using our BFS traversal
            var ancestors = GetExecutionScopeForNode(node.Id);

            foreach (var binding in node.InputBindings)
            {
                // 1. Check Execution Scope Reachability
                if (!ancestors.Contains(binding.Source.SourceNodeId))
                    return Result.Failure($"Invalid mapping on Node '{node.Name}': Source Node '{binding.Source.SourceNodeId}' is not an upstream ancestor.");

                // 2. Strict Type Checking
                var sourceNode = GetNode(binding.Source.SourceNodeId);
                var sourceOutput = sourceNode.OutputDefinitions.FirstOrDefault(o => o.PropertyName == binding.Source.SourcePath);
                var targetInput = node.InputDefinitions.FirstOrDefault(i => i.PropertyName == binding.TargetInputProperty);

                if (sourceOutput == null || targetInput == null)
                    return Result.Failure($"Invalid mapping on Node '{node.Name}': Source or Target property not found in the node contracts.");

                if (!sourceOutput.DataType.IsAssignableTo(targetInput.DataType))
                {
                    return Result.Failure($"Type mismatch on Node '{node.Name}'. Cannot map {sourceOutput.DataType} to {targetInput.DataType}.");
                }
            }
        }

        // Validate Mapped technical properties (generic iteration over TechnicalInputs)
        foreach (var node in _nodes)
        {
            var ancestors = GetExecutionScopeForNode(node.Id);

            // Iterate over all technical inputs regardless of node type
            foreach (var (propertyName, valueSource) in node.TechnicalInputs)
            {
                if (valueSource is InputValueSource.Mapped mapped)
                {
                    var validationResult = ValidateMappedProperty(node.Name, propertyName, mapped.Source, ancestors);
                    if (validationResult.IsFailure)
                        return validationResult;
                }
            }
        }

        return Result.Success();
    }

    private Result ValidateMappedProperty(string nodeName, string propertyName, VariableReference source, HashSet<WorkflowNodeId> ancestors)
    {
        // 1. Check Execution Scope Reachability
        if (!ancestors.Contains(source.SourceNodeId))
            return Result.Failure($"Invalid mapped property '{propertyName}' on Node '{nodeName}': Source Node '{source.SourceNodeId}' is not an upstream ancestor.");

        // 2. Type Check: Must be String type for technical properties
        var sourceNode = GetNode(source.SourceNodeId);
        var sourceOutput = sourceNode.OutputDefinitions.FirstOrDefault(o => o.PropertyName == source.SourcePath);

        if (sourceOutput == null)
            return Result.Failure($"Invalid mapped property '{propertyName}' on Node '{nodeName}': Source property '{source.SourcePath}' not found in source node.");

        if (sourceOutput.DataType is not WorkflowDataType.Primitive primitive || primitive != WorkflowDataType.Primitive.String)
        {
            return Result.Failure($"Type mismatch on mapped property '{propertyName}' on Node '{nodeName}'. Technical properties require String type, but source is {sourceOutput.DataType}.");
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
