using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.WorkflowOrchestration;

public sealed record WorkflowNodeId
{
    private const string Abbreviation = "WFN";
    public Guid Value { get; }

    private WorkflowNodeId(Guid value) => Value = value;

    public static Result<WorkflowNodeId> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Result<WorkflowNodeId>.Failure("WorkflowNode ID cannot be null or empty");

        var expectedPrefix = $"{Abbreviation}Id";
        if (!value.StartsWith(expectedPrefix))
            return Result<WorkflowNodeId>.Failure($"WorkflowNode ID must start with '{expectedPrefix}'");

        var guidString = value.Substring(expectedPrefix.Length);
        if (!Guid.TryParse(guidString, out var guid))
            return Result<WorkflowNodeId>.Failure("Invalid GUID format in WorkflowNode ID");

        return Result<WorkflowNodeId>.Success(new WorkflowNodeId(guid));
    }

    public static WorkflowNodeId New() => new WorkflowNodeId(Guid.CreateVersion7());

    public override string ToString() => $"{Abbreviation}Id{Value}";
}
