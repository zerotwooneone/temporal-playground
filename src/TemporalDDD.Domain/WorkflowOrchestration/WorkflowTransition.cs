namespace TemporalDDD.Domain.WorkflowOrchestration;

public sealed record WorkflowTransition(WorkflowNodeId SourceNodeId, WorkflowNodeId TargetNodeId);
