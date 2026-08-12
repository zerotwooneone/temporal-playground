namespace TemporalDDD.Domain.WorkflowOrchestration;

// Represents a pointer to a specific output property of an ancestor node
public record VariableReference(WorkflowNodeId SourceNodeId, string SourcePath);

// Maps a specific property on the current node's input to that reference
public record ParameterBinding(string TargetInputProperty, VariableReference Source);
