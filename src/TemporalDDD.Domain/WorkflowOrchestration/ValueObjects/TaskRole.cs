using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

public sealed record TaskRole
{
    public string Value { get; }

    private TaskRole(string value) => Value = value;

    public static Result<TaskRole> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<TaskRole>.Failure("Task role cannot be null or empty");

        return Result<TaskRole>.Success(new TaskRole(value.Trim()));
    }

    public override string ToString() => Value;
}
