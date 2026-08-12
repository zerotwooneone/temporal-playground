namespace TemporalDDD.Domain.WorkflowOrchestration.Nodes;

using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

public interface IActivityWorkflowNode
{
    RetryPolicy? RetryPolicy { get; }
}
