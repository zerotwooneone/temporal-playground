using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

namespace TemporalDDD.Domain.Tests.WorkflowOrchestration.ValueObjects;

public class RetryPolicyTests
{
    [Fact]
    public void Create_WithValidMaxAttemptsAndBackoff_ReturnsSuccess()
    {
        // ARRANGE
        var maxAttempts = 3;
        var backoffCoefficient = 2;

        // ACT
        var result = RetryPolicy.Create(maxAttempts, backoffCoefficient);

        // ASSERT
        result.IsSuccess.Should().BeTrue();
        result.Value.MaxAttempts.Should().Be(maxAttempts);
        result.Value.BackoffCoefficient.Should().Be(backoffCoefficient);
    }

    [Fact]
    public void Create_WithMaxAttemptsLessThanOne_ReturnsFailure()
    {
        // ARRANGE
        var maxAttempts = 0;
        var backoffCoefficient = 2;

        // ACT
        var result = RetryPolicy.Create(maxAttempts, backoffCoefficient);

        // ASSERT
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("MaxAttempts must be greater than or equal to 1");
    }

    [Fact]
    public void Create_WithMaxAttemptsNegative_ReturnsFailure()
    {
        // ARRANGE
        var maxAttempts = -1;
        var backoffCoefficient = 2;

        // ACT
        var result = RetryPolicy.Create(maxAttempts, backoffCoefficient);

        // ASSERT
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("MaxAttempts must be greater than or equal to 1");
    }

    [Fact]
    public void Create_WithBackoffCoefficientLessThanOne_ReturnsFailure()
    {
        // ARRANGE
        var maxAttempts = 3;
        var backoffCoefficient = 0;

        // ACT
        var result = RetryPolicy.Create(maxAttempts, backoffCoefficient);

        // ASSERT
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("BackoffCoefficient must be greater than or equal to 1");
    }

    [Fact]
    public void Create_WithBackoffCoefficientNegative_ReturnsFailure()
    {
        // ARRANGE
        var maxAttempts = 3;
        var backoffCoefficient = -1;

        // ACT
        var result = RetryPolicy.Create(maxAttempts, backoffCoefficient);

        // ASSERT
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("BackoffCoefficient must be greater than or equal to 1");
    }

    [Fact]
    public void ToString_ReturnsCorrectFormat()
    {
        // ARRANGE
        var retryPolicy = RetryPolicy.Create(3, 2).Value!;

        // ACT
        var result = retryPolicy.ToString();

        // ASSERT
        result.Should().Be("MaxAttempts: 3, BackoffCoefficient: 2");
    }
}
