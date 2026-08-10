using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.WorkflowOrchestration;

public sealed record WorkflowInstanceStatus
{
    public int Value { get; }
    public string Name { get; }

    private WorkflowInstanceStatus(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public static readonly WorkflowInstanceStatus Running = new(1, "Running");
    public static readonly WorkflowInstanceStatus Completed = new(2, "Completed");
    public static readonly WorkflowInstanceStatus Failed = new(3, "Failed");
    public static readonly WorkflowInstanceStatus Suspended = new(4, "Suspended");

    private static readonly WorkflowInstanceStatus[] AllStatuses = { Running, Completed, Failed, Suspended };

    public static Result<WorkflowInstanceStatus> FromValue(int value)
    {
        var status = AllStatuses.FirstOrDefault(s => s.Value == value);
        if (status == null)
            return Result<WorkflowInstanceStatus>.Failure($"Invalid WorkflowInstanceStatus value: {value}");
        return Result<WorkflowInstanceStatus>.Success(status);
    }

    public static implicit operator int(WorkflowInstanceStatus status) => status.Value;

    public override string ToString() => Name;
}
