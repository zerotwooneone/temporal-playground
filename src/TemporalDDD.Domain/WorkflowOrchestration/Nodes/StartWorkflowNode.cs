namespace TemporalDDD.Domain.WorkflowOrchestration.Nodes;

public sealed class StartWorkflowNode : WorkflowNode
{
    private StartWorkflowNode() { }

    private StartWorkflowNode(WorkflowNodeId id, string name, string? businessNotes)
        : base(id, NodeType.Start, name, businessNotes)
    {
    }

    // Internal constructor for infrastructure rehydration
    internal StartWorkflowNode(
        WorkflowNodeId id,
        string name,
        string? businessNotes,
        bool isConfigured)
        : base(id, NodeType.Start, name, businessNotes)
    {
        IsConfigured = isConfigured;
    }

    public static StartWorkflowNode CreateStub(string name, string? businessNotes)
    {
        var node = new StartWorkflowNode(WorkflowNodeId.New(), name, businessNotes);
        node.IsConfigured = true; // Start nodes are always configured
        return node;
    }

    public override void ValidateConfiguration()
    {
        IsConfigured = true; // Start nodes are always configured
    }
}
