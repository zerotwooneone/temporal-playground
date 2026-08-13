namespace TemporalDDD.Domain.WorkflowOrchestration;

// Port/Terminal Model: Every transition explicitly specifies the SourcePort on the source node
public sealed record WorkflowTransition(WorkflowNodeId SourceNodeId, WorkflowNodeId TargetNodeId, string SourcePort);
