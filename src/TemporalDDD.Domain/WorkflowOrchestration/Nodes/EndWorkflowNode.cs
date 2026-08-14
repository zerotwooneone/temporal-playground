namespace TemporalDDD.Domain.WorkflowOrchestration.Nodes;

public sealed class EndWorkflowNode : WorkflowNode
{
    private EndWorkflowNode(WorkflowNodeId id, string name, string? businessNotes)
        : base(id, NodeType.End, name, businessNotes)
    {
        // End nodes have no output ports (terminal node)
    }

    // Internal constructor for infrastructure rehydration
    internal EndWorkflowNode(
        WorkflowNodeId id,
        string name,
        string? businessNotes,
        bool isConfigured)
        : base(id, NodeType.End, name, businessNotes)
    {
        IsConfigured = isConfigured;
        // End nodes have no output ports (terminal node)
    }

    public static EndWorkflowNode CreateStub(string name, string? businessNotes, WorkflowNodeId? id = null)
    {
        var node = new EndWorkflowNode(id ?? WorkflowNodeId.New(), name, businessNotes);
        node.IsConfigured = true; // End nodes are always configured
        return node;
    }

    public override void ValidateConfiguration(IReadOnlyList<WorkflowTransition> transitions)
    {
        // Transitions parameter is not used for End node configuration
        IsConfigured = true; // End nodes are always configured
    }
}
