using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.WorkflowOrchestration;

public sealed record NodeType
{
    public int Value { get; }
    public string Name { get; }

    private NodeType(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public static readonly NodeType Api = new(1, "Api");
    public static readonly NodeType Notification = new(2, "Notification");
    public static readonly NodeType HumanTask = new(3, "HumanTask");
    public static readonly NodeType Delay = new(4, "Delay");
    public static readonly NodeType Decision = new(5, "Decision");
    public static readonly NodeType DataTransformation = new(6, "DataTransformation");

    private static readonly NodeType[] AllTypes = { Api, Notification, HumanTask, Delay, Decision, DataTransformation };

    public static Result<NodeType> FromValue(int value)
    {
        var type = AllTypes.FirstOrDefault(t => t.Value == value);
        if (type == null)
            return Result<NodeType>.Failure($"Invalid NodeType value: {value}");
        return Result<NodeType>.Success(type);
    }

    public static implicit operator int(NodeType type) => type.Value;

    public override string ToString() => Name;
}
