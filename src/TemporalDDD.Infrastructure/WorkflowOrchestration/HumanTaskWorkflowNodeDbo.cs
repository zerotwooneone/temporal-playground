namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public class HumanTaskWorkflowNodeDbo : WorkflowNodeDbo
{
    // Flattened Value Objects
    public string? RequiredRole { get; set; }
    public string? SignalName { get; set; }
    public int? TimeoutInMinutes { get; set; }

    // Additional properties
    public string? UIFormSchema { get; set; }
}
