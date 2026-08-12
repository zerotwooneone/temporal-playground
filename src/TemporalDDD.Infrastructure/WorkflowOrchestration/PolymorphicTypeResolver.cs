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

        if (type == typeof(WorkflowDataType))
        {
            typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$type",
                DerivedTypes =
                {
                    new JsonDerivedType(typeof(WorkflowDataType.Primitive), "Primitive"),
                    new JsonDerivedType(typeof(WorkflowDataType.Semantic), "Semantic")
                }
            };
        }

        return typeInfo;
    }
}
