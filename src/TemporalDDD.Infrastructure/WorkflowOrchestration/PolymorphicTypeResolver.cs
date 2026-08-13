using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using TemporalDDD.Domain.WorkflowOrchestration;

namespace TemporalDDD.Infrastructure.WorkflowOrchestration;

public class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
{
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        JsonTypeInfo typeInfo = base.GetTypeInfo(type, options);

        if (type == typeof(WorkflowDataTypeDto))
        {
            typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "TypeDiscriminator",
                DerivedTypes =
                {
                    new JsonDerivedType(typeof(WorkflowDataTypeDto.PrimitiveDto), "Primitive"),
                    new JsonDerivedType(typeof(WorkflowDataTypeDto.SemanticDto), "Semantic")
                }
            };
        }

        if (type == typeof(InputValueSource))
        {
            typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$type",
                DerivedTypes =
                {
                    new JsonDerivedType(typeof(InputValueSource.Fixed), "Fixed"),
                    new JsonDerivedType(typeof(InputValueSource.Mapped), "Mapped")
                }
            };
        }

        return typeInfo;
    }
}
