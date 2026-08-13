using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration.Nodes;
using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

namespace TemporalDDD.Domain.Tests.Builders;

public class WorkflowBuilder
{
    private readonly WorkflowDefinition _workflow;
    private readonly Dictionary<string, WorkflowNodeId> _nodes = new();
    private readonly List<WorkflowNode> _additionalNodes = new();
    private readonly List<WorkflowTransition> _transitions = new();

    public WorkflowBuilder()
    {
        var publicId = WorkflowDefinitionPublicId.New();
        _workflow = WorkflowDefinition.Create(UserId.New(), "Test Workflow", "{}", publicId);
        
        // Track the default Start and End nodes
        var startNode = _workflow.Nodes.OfType<StartWorkflowNode>().First();
        var endNode = _workflow.Nodes.OfType<EndWorkflowNode>().First();
        _nodes["Start"] = startNode.Id;
        _nodes["End"] = endNode.Id;
    }

    public WorkflowBuilder WithWorkflowInput(NodeOutputDefinition inputDefinition)
    {
        _workflow.UpdateWorkflowInputs(new[] { inputDefinition });
        return this;
    }

    public WorkflowBuilder WithApiNode(
        string referenceName, 
        out WorkflowNodeId nodeId, 
        bool skipConfiguration = false)
    {
        var node = ApiWorkflowNode.CreateStub(referenceName, null);
        
        if (!skipConfiguration)
        {
            // Apply valid defaults so the test doesn't fail IsConfigured checks
            var retryPolicy = RetryPolicy.Create(3, 2).Value!;
            var contractMapping = ContractMapping.Create(true, null, null, null).Value!;
            node.SetTechnicalInput(ApiWorkflowNode.EndpointUrlKey, new InputValueSource.Fixed("https://default.api"));
            node.SetTechnicalInput(ApiWorkflowNode.AuthTokenKey, new InputValueSource.Fixed("token"));
            node.ConfigureValueObjects(retryPolicy, contractMapping);
        }

        _additionalNodes.Add(node);
        _nodes[referenceName] = node.Id;
        nodeId = node.Id;
        return this;
    }

    public WorkflowBuilder WithNotificationNode(
        string referenceName, 
        out WorkflowNodeId nodeId,
        bool skipConfiguration = false)
    {
        var node = NotificationWorkflowNode.CreateStub(referenceName, null);
        
        if (!skipConfiguration)
        {
            // Apply valid defaults so the test doesn't fail IsConfigured checks
            node.SetTechnicalInput(NotificationWorkflowNode.MessageTemplateKey, new InputValueSource.Fixed("Default message template"));
        }

        _additionalNodes.Add(node);
        _nodes[referenceName] = node.Id;
        nodeId = node.Id;
        return this;
    }

    public WorkflowBuilder WithDecisionNode(
        string referenceName, 
        out WorkflowNodeId nodeId)
    {
        var node = DecisionWorkflowNode.CreateStub(referenceName, null);
        
        // Decision nodes are always configured once created
        node.ValidateConfiguration();

        _additionalNodes.Add(node);
        _nodes[referenceName] = node.Id;
        nodeId = node.Id;
        return this;
    }

    public WorkflowBuilder WithHumanTaskNode(
        string referenceName, 
        out WorkflowNodeId nodeId,
        IEnumerable<NodeInputDefinition>? inputs = null,
        IEnumerable<NodeOutputDefinition>? outputs = null,
        bool skipConfiguration = false)
    {
        var node = HumanTaskWorkflowNode.CreateStub(referenceName, null);
        
        if (inputs != null)
        {
            node.ConfigureInputs(inputs);
        }
        
        if (outputs != null)
        {
            node.ConfigureOutputs(outputs);
        }
        
        if (!skipConfiguration)
        {
            // Apply valid defaults so the test doesn't fail IsConfigured checks
            var requiredRole = TaskRole.Create("Admin").Value!;
            var signalName = TemporalSignalName.Create("HumanTaskSignal").Value!;
            var timeout = TaskTimeout.Create(30).Value!;
            node.ConfigureTechnicalDetails(requiredRole, signalName, timeout, null);
        }

        _additionalNodes.Add(node);
        _nodes[referenceName] = node.Id;
        nodeId = node.Id;
        return this;
    }

    public WorkflowBuilder WithTransition(string sourceReference, string targetReference, string? branchLabel = null)
    {
        if (!_nodes.ContainsKey(sourceReference))
            throw new ArgumentException($"Source node '{sourceReference}' not found");
        if (!_nodes.ContainsKey(targetReference))
            throw new ArgumentException($"Target node '{targetReference}' not found");

        var sourceId = _nodes[sourceReference];
        var targetId = _nodes[targetReference];
        
        _transitions.Add(new WorkflowTransition(sourceId, targetId, branchLabel ?? "Default"));
        return this;
    }

    public WorkflowBuilder WithTransition(WorkflowNodeId sourceId, WorkflowNodeId targetId, string? branchLabel = null)
    {
        _transitions.Add(new WorkflowTransition(sourceId, targetId, branchLabel ?? "Default"));
        return this;
    }

    public WorkflowBuilder WithDataMapping(string targetNodeReference, string targetProperty, string sourceNodeReference, string sourceProperty)
    {
        if (!_nodes.ContainsKey(targetNodeReference))
            throw new ArgumentException($"Target node '{targetNodeReference}' not found");
        if (!_nodes.ContainsKey(sourceNodeReference))
            throw new ArgumentException($"Source node '{sourceNodeReference}' not found");

        var targetNodeId = _nodes[targetNodeReference];
        var sourceNodeId = _nodes[sourceNodeReference];

        var binding = new ParameterBinding(targetProperty, new VariableReference(sourceNodeId, sourceProperty));
        
        // Find the target node and update its bindings
        var allNodes = _workflow.Nodes.Concat(_additionalNodes).ToList();
        var targetNode = allNodes.FirstOrDefault(n => n.Id == targetNodeId);
        if (targetNode == null)
            throw new ArgumentException($"Target node with ID '{targetNodeId}' not found");

        targetNode.UpdateInputBindings(new[] { binding });
        
        return this;
    }

    public WorkflowBuilder ReadyForApproval()
    {
        // Build the complete node and transition lists
        var allNodes = _workflow.Nodes.Concat(_additionalNodes).ToList();
        _workflow.UpdateNodes(allNodes, _transitions, null);
        _workflow.SubmitForReview();
        return this;
    }

    public WorkflowDefinition Build()
    {
        // Build the complete node and transition lists if not already done
        if (_additionalNodes.Any() || _transitions.Any())
        {
            var allNodes = _workflow.Nodes.Concat(_additionalNodes).ToList();
            _workflow.UpdateNodes(allNodes, _transitions, null);
        }
        return _workflow;
    }
}
