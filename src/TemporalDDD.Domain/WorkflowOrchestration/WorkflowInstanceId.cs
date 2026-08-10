using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.WorkflowOrchestration;

public sealed record WorkflowInstanceId
{
    private const string Abbreviation = "WFI";
    public Guid Value { get; }

    private WorkflowInstanceId(Guid value) => Value = value;

    public static Result<WorkflowInstanceId> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Result<WorkflowInstanceId>.Failure("WorkflowInstance ID cannot be null or empty");

        var expectedPrefix = $"{Abbreviation}Id";
        if (!value.StartsWith(expectedPrefix))
            return Result<WorkflowInstanceId>.Failure($"WorkflowInstance ID must start with '{expectedPrefix}'");

        var guidString = value.Substring(expectedPrefix.Length);
        if (!Guid.TryParse(guidString, out var guid))
            return Result<WorkflowInstanceId>.Failure("Invalid GUID format in WorkflowInstance ID");

        return Result<WorkflowInstanceId>.Success(new WorkflowInstanceId(guid));
    }

    public static WorkflowInstanceId New() => new WorkflowInstanceId(Guid.CreateVersion7());

    public override string ToString() => $"{Abbreviation}Id{Value}";
}
