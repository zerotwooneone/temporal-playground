using TemporalDDD.Domain.WorkflowOrchestration;

namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public class WorkflowDefinitionDbo
{
    public string Id { get; set; }
    public string PublicId { get; set; }
    public string CreatorId { get; set; }
    public string Name { get; set; }
    public string ClassName { get; set; }
    public int Status { get; set; }
    public string FlowJson { get; set; }
    
    // JSON column for Workflow Inputs
    public string ExpectedInputsJson { get; set; } = "[]";
}
