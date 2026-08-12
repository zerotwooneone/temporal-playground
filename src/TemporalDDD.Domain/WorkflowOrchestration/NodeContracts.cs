namespace TemporalDDD.Domain.WorkflowOrchestration;

public record NodeInputDefinition(string PropertyName, WorkflowDataType DataType, bool IsRequired);
public record NodeOutputDefinition(string PropertyName, WorkflowDataType DataType);
