namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public class WorkflowTransitionDbo
{
    public int Id { get; set; }
    public string WorkflowDefinitionId { get; set; }
    public string SourceNodeId { get; set; }
    public string TargetNodeId { get; set; }
    public string SourcePort { get; set; } = "Default";
}
