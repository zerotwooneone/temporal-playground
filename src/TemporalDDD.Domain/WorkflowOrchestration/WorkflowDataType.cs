namespace TemporalDDD.Domain.WorkflowOrchestration;

public abstract record WorkflowDataType
{
    // The Base Primitives (Using our Smart Enum style)
    public sealed record Primitive : WorkflowDataType
    {
        public int Value { get; }
        public string Name { get; }

        private Primitive(int value, string name)
        {
            Value = value;
            Name = name;
        }

        public static readonly Primitive String = new(1, "String");
        public static readonly Primitive Number = new(2, "Number");
        public static readonly Primitive Boolean = new(3, "Boolean");
        public static readonly Primitive Date = new(4, "Date");
        public static readonly Primitive Json = new(5, "JsonDocument");
    }

    // The Semantic Wrapper (e.g., new Semantic("UserId", Primitive.String))
    public sealed record Semantic : WorkflowDataType
    {
        public string SemanticName { get; }
        public Primitive UnderlyingType { get; }

        public Semantic(string semanticName, Primitive underlyingType)
        {
            SemanticName = semanticName;
            UnderlyingType = underlyingType;
        }
    }

    // Strict Type Compatibility Rule (No implicit coercion)
    public bool IsAssignableTo(WorkflowDataType targetType)
    {
        // 1. Exact match
        if (this == targetType) return true;

        // 2. A specific Semantic type can safely plug into its generic Primitive base
        if (this is Semantic semanticSource && targetType is Primitive primitiveTarget)
        {
            return semanticSource.UnderlyingType == primitiveTarget;
        }

        return false;
    }
}
