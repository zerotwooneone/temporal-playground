using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.WorkflowOrchestration;

public sealed record WorkflowStatus
{
    public int Value { get; }
    public string Name { get; }

    private WorkflowStatus(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public static readonly WorkflowStatus Draft = new(0, "Draft");
    public static readonly WorkflowStatus PendingReview = new(1, "PendingReview");
    public static readonly WorkflowStatus Approved = new(2, "Approved");
    public static readonly WorkflowStatus Rejected = new(3, "Rejected");

    private static readonly WorkflowStatus[] AllStatuses = { Draft, PendingReview, Approved, Rejected };

    public static Result<WorkflowStatus> FromValue(int value)
    {
        var status = AllStatuses.FirstOrDefault(s => s.Value == value);
        if (status == null)
            return Result<WorkflowStatus>.Failure($"Invalid WorkflowStatus value: {value}");
        return Result<WorkflowStatus>.Success(status);
    }

    public static implicit operator int(WorkflowStatus status) => status.Value;

    public override string ToString() => Name;
}
