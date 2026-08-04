using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

public sealed record RetryPolicy
{
    public int MaxAttempts { get; }
    public int BackoffCoefficient { get; }

    private RetryPolicy(int maxAttempts, int backoffCoefficient)
    {
        MaxAttempts = maxAttempts;
        BackoffCoefficient = backoffCoefficient;
    }

    public static Result<RetryPolicy> Create(int maxAttempts, int backoffCoefficient)
    {
        if (maxAttempts < 1)
            return Result<RetryPolicy>.Failure("MaxAttempts must be greater than or equal to 1");

        if (backoffCoefficient < 1)
            return Result<RetryPolicy>.Failure("BackoffCoefficient must be greater than or equal to 1");

        return Result<RetryPolicy>.Success(new RetryPolicy(maxAttempts, backoffCoefficient));
    }

    public override string ToString() => $"MaxAttempts: {MaxAttempts}, BackoffCoefficient: {BackoffCoefficient}";
}
