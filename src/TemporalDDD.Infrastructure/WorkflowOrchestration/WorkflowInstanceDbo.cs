namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public class WorkflowInstanceDbo
{
    public string Id { get; set; }
    public string PublicId { get; set; }
    public string WorkflowDefinitionId { get; set; }
    public string BusinessReferenceId { get; set; }
    public string TemporalRunId { get; set; }
    public int Status { get; set; }
    public string ContextData { get; set; }
}
