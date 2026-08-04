namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public abstract class WorkflowNodeDbo
{
    public string Id { get; set; }
    public string WorkflowDefinitionId { get; set; }
    public int NodeType { get; set; }
    public string Name { get; set; }
    public string? BusinessNotes { get; set; }
    public bool IsConfigured { get; set; }
}
