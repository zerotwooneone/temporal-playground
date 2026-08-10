using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.WorkflowOrchestration;

public sealed record WorkflowInstancePublicId
{
    private const string Abbreviation = "WFIPub";
    public Guid Value { get; }

    private WorkflowInstancePublicId(Guid value) => Value = value;

    public static Result<WorkflowInstancePublicId> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Result<WorkflowInstancePublicId>.Failure("WorkflowInstance Public ID cannot be null or empty");

        var expectedPrefix = $"{Abbreviation}Id";
        if (!value.StartsWith(expectedPrefix))
            return Result<WorkflowInstancePublicId>.Failure($"WorkflowInstance Public ID must start with '{expectedPrefix}'");

        var guidString = value.Substring(expectedPrefix.Length);
        if (!Guid.TryParse(guidString, out var guid))
            return Result<WorkflowInstancePublicId>.Failure("Invalid GUID format in WorkflowInstance Public ID");

        return Result<WorkflowInstancePublicId>.Success(new WorkflowInstancePublicId(guid));
    }

    public static WorkflowInstancePublicId New() => new WorkflowInstancePublicId(Guid.NewGuid());

    public override string ToString() => $"{Abbreviation}Id{Value}";
}
