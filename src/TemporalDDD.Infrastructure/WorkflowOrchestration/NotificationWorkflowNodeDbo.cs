namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public class NotificationWorkflowNodeDbo : WorkflowNodeDbo
{
    // Unified technical inputs as JSON
    public string? TechnicalInputsJson { get; set; }
}
