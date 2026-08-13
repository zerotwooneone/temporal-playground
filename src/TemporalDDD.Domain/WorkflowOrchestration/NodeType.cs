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

    public static readonly NodeType Start = new(0, "Start");
    public static readonly NodeType Api = new(1, "Api");
    public static readonly NodeType Notification = new(2, "Notification");
    public static readonly NodeType Decision = new(3, "Decision");
    public static readonly NodeType HumanTask = new(4, "HumanTask");
    public static readonly NodeType End = new(99, "End");

    private static readonly NodeType[] AllTypes = { Start, Api, Notification, Decision, HumanTask, End };

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
