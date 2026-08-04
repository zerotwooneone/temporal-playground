namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public class WorkflowDefinitionDbo
{
    public string Id { get; set; }
    public string PublicId { get; set; }
    public string CreatorId { get; set; }
    public string Name { get; set; }
    public int Status { get; set; }
    public string FlowJson { get; set; }
}
