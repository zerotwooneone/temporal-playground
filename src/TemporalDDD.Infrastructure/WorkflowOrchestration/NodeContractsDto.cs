using TemporalDDD.Domain.WorkflowOrchestration;

namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

// DTOs for JSON serialization of node contracts
public record NodeInputDefinitionDto(string PropertyName, WorkflowDataTypeDto DataType, bool IsRequired);
public record NodeOutputDefinitionDto(string PropertyName, WorkflowDataTypeDto DataType);

public static class NodeContractsDtoMapper
{
    public static NodeInputDefinitionDto ToDto(NodeInputDefinition definition) =>
        new(definition.PropertyName, WorkflowDataTypeDto.FromDomain(definition.DataType), definition.IsRequired);

    public static NodeOutputDefinitionDto ToDto(NodeOutputDefinition definition) =>
        new(definition.PropertyName, WorkflowDataTypeDto.FromDomain(definition.DataType));

    public static NodeInputDefinition ToDomain(NodeInputDefinitionDto dto) =>
        new(dto.PropertyName, dto.DataType.ToDomain(), dto.IsRequired);

    public static NodeOutputDefinition ToDomain(NodeOutputDefinitionDto dto) =>
        new(dto.PropertyName, dto.DataType.ToDomain());
}
