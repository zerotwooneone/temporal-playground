namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public class DecisionWorkflowNodeDbo : WorkflowNodeDbo
{
    // Decision nodes have no additional properties beyond the base
    // The fixed input (Condition: Boolean) is handled in the domain model
}
