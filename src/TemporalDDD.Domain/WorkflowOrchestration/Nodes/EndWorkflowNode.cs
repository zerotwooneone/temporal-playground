namespace TemporalDDD.Domain.WorkflowOrchestration.Nodes;

public sealed class EndWorkflowNode : WorkflowNode
{
    private EndWorkflowNode() { }

    private EndWorkflowNode(WorkflowNodeId id, string name, string? businessNotes)
        : base(id, NodeType.End, name, businessNotes)
    {
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
    }

    public static EndWorkflowNode CreateStub(string name, string? businessNotes)
    {
        return new EndWorkflowNode(WorkflowNodeId.New(), name, businessNotes);
    }

    public override void ValidateConfiguration()
    {
        IsConfigured = true; // End nodes are always configured
    }
}
