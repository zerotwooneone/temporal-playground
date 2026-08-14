namespace TemporalDDD.Domain.WorkflowOrchestration.Nodes;

using TemporalDDD.Domain.WorkflowOrchestration;

public class DecisionWorkflowNode : WorkflowNode
{
    private DecisionWorkflowNode(WorkflowNodeId id, string name, string? businessNotes) 
        : base(id, NodeType.Decision, name, businessNotes)
    {
        // A Decision Node REQUIRES a Boolean input to evaluate
        _inputDefinitions.Add(new NodeInputDefinition("Condition", WorkflowDataType.Primitive.Boolean, true));
        // Decision nodes have two output ports: True and False
        _outputPorts.Add("True");
        _outputPorts.Add("False");
    }

    // Internal constructor for infrastructure rehydration
    internal DecisionWorkflowNode(
        WorkflowNodeId id,
        string name,
        string? businessNotes,
        bool isConfigured)
        : base(id, NodeType.Decision, name, businessNotes)
    {
        IsConfigured = isConfigured;
        // Rehydrate the fixed input definition
        _inputDefinitions.Add(new NodeInputDefinition("Condition", WorkflowDataType.Primitive.Boolean, true));
        // Decision nodes have two output ports: True and False
        _outputPorts.Add("True");
        _outputPorts.Add("False");
    }

    public static DecisionWorkflowNode CreateStub(string name, string? businessNotes, WorkflowNodeId? id = null)
    {
        var node = new DecisionWorkflowNode(id ?? WorkflowNodeId.New(), name, businessNotes);
        node._outputPorts.Add("True");
        node._outputPorts.Add("False");
        return node;
    }

    public override void ValidateConfiguration(IReadOnlyList<WorkflowTransition> transitions)
    {
        // Decision nodes are configured if either True or False port has at least one outgoing transition
        var hasTrueTransition = transitions.Any(t => t.SourceNodeId == Id && t.SourcePort == "True");
        var hasFalseTransition = transitions.Any(t => t.SourceNodeId == Id && t.SourcePort == "False");

        IsConfigured = hasTrueTransition || hasFalseTransition;
    }
}
