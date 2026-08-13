using System.Text.Json.Serialization;
using TemporalDDD.Domain.WorkflowOrchestration;

namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

// DTO for JSON serialization of WorkflowDataType
public abstract record WorkflowDataTypeDto
{
    public string TypeDiscriminator { get; init; } = null!;

    public sealed record PrimitiveDto : WorkflowDataTypeDto
    {
        public int Value { get; init; }
        public string Name { get; init; }

        [JsonConstructor]
        public PrimitiveDto(int value, string name)
        {
            TypeDiscriminator = "Primitive";
            Value = value;
            Name = name;
        }

        public static PrimitiveDto FromDomain(WorkflowDataType.Primitive primitive) => new(primitive.Value, primitive.Name);
        public WorkflowDataType.Primitive ToDomain() => Name switch
        {
            "String" => WorkflowDataType.Primitive.String,
            "Number" => WorkflowDataType.Primitive.Number,
            "Boolean" => WorkflowDataType.Primitive.Boolean,
            "Date" => WorkflowDataType.Primitive.Date,
            "JsonDocument" => WorkflowDataType.Primitive.Json,
            _ => throw new InvalidOperationException($"Unknown primitive type: {Name}")
        };
    }

    public sealed record SemanticDto : WorkflowDataTypeDto
    {
        public string SemanticName { get; init; }
        public PrimitiveDto UnderlyingType { get; init; } = null!;

        [JsonConstructor]
        public SemanticDto(string semanticName, PrimitiveDto underlyingType)
        {
            TypeDiscriminator = "Semantic";
            SemanticName = semanticName;
            UnderlyingType = underlyingType;
        }

        public static SemanticDto FromDomain(WorkflowDataType.Semantic semantic) => 
            new(semantic.SemanticName, PrimitiveDto.FromDomain(semantic.UnderlyingType));

        public WorkflowDataType.Semantic ToDomain() => 
            new(SemanticName, UnderlyingType.ToDomain());
    }

    public static WorkflowDataTypeDto FromDomain(WorkflowDataType dataType) => dataType switch
    {
        WorkflowDataType.Primitive primitive => PrimitiveDto.FromDomain(primitive),
        WorkflowDataType.Semantic semantic => SemanticDto.FromDomain(semantic),
        _ => throw new InvalidOperationException($"Unknown WorkflowDataType: {dataType.GetType()}")
    };

    public WorkflowDataType ToDomain() => this switch
    {
        PrimitiveDto primitive => primitive.ToDomain(),
        SemanticDto semantic => semantic.ToDomain(),
        _ => throw new InvalidOperationException($"Unknown WorkflowDataTypeDto: {GetType()}")
    };
}
