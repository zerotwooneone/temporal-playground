using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

public sealed record TaskTimeout
{
    public int TimeoutInMinutes { get; }

    private TaskTimeout(int timeoutInMinutes) => TimeoutInMinutes = timeoutInMinutes;

    public static Result<TaskTimeout> Create(int timeoutInMinutes)
    {
        if (timeoutInMinutes <= 0)
            return Result<TaskTimeout>.Failure("Task timeout must be greater than 0 minutes");

        if (timeoutInMinutes > 43200)
            return Result<TaskTimeout>.Failure("Task timeout cannot exceed 43200 minutes (30 days)");

        return Result<TaskTimeout>.Success(new TaskTimeout(timeoutInMinutes));
    }

    public override string ToString() => $"{TimeoutInMinutes} minutes";
}
