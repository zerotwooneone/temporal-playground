namespace TemporalDDD.Domain.WorkflowOrchestration;

// Add an optional BranchLabel (e.g., "True", "False")
public sealed record WorkflowTransition(WorkflowNodeId SourceNodeId, WorkflowNodeId TargetNodeId, string? BranchLabel = null);
