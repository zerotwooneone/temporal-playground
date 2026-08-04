namespace TemporalDDD.Domain.WorkflowOrchestration.Nodes;

public abstract class WorkflowNode
{
    public WorkflowNodeId Id { get; private set; }
    public NodeType Type { get; private set; }
    public string Name { get; private set; }
    public string? BusinessNotes { get; private set; }
    public bool IsConfigured { get; protected set; }

    protected WorkflowNode() { }

    protected WorkflowNode(WorkflowNodeId id, NodeType type, string name, string? businessNotes)
    {
        Id = id;
        Type = type;
        Name = name;
        BusinessNotes = businessNotes;
        IsConfigured = false;
    }

    public void UpdateBusinessIntent(string name, string? businessNotes)
    {
        Name = name;
        BusinessNotes = businessNotes;
    }

    public abstract void ValidateConfiguration();
}
