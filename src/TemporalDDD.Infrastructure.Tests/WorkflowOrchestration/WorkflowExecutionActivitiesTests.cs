using System.Text.Json;
using TemporalDDD.Infrastructure.Testing;
using TemporalDDD.Infrastructure.WorkflowOrchestration.Activities;

namespace TemporalDDD.Infrastructure.Tests.WorkflowOrchestration;

public class WorkflowExecutionActivitiesTests
{
    private readonly ChaosHttpClient _chaosHttpClient;
    private readonly WorkflowExecutionActivities _activities;

    public WorkflowExecutionActivitiesTests()
    {
        var random = new Random(42); // Fixed seed for reproducible tests
        _chaosHttpClient = new ChaosHttpClient(random);
        _activities = new WorkflowExecutionActivities(_chaosHttpClient);
    }

    #region ExecuteApiCallAsync Tests
    [Fact]
    public async Task ExecuteApiCallAsync_WhenEndpointUrlIsValid_ReturnsJsonDocument()
    {
        // Arrange
        var input = new ExecuteApiInput(
            NodeId: "test-node-1",
            EndpointUrl: "https://api.example.com/test",
            AuthToken: null,
            Mapping: null
        );

        // Act
        var result = await _activities.ExecuteApiCallAsync(input);

        // Assert
        result.Should().NotBeNull();
        result.RootElement.ValueKind.Should().Be(JsonValueKind.Object);
    }

    [Fact]
    public async Task ExecuteApiCallAsync_WhenEndpointUrlIsEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        var input = new ExecuteApiInput(
            NodeId: "test-node-2",
            EndpointUrl: null,
            AuthToken: null,
            Mapping: null
        );

        // Act
        var act = async () => await _activities.ExecuteApiCallAsync(input);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*EndpointUrl is required*");
    }

    [Fact]
    public async Task ExecuteApiCallAsync_WhenEndpointUrlIsWhitespace_ThrowsInvalidOperationException()
    {
        // Arrange
        var input = new ExecuteApiInput(
            NodeId: "test-node-3",
            EndpointUrl: "   ",
            AuthToken: null,
            Mapping: null
        );

        // Act
        var act = async () => await _activities.ExecuteApiCallAsync(input);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*EndpointUrl is required*");
    }
    #endregion

    #region SendNotificationAsync Tests
    [Fact]
    public async Task SendNotificationAsync_WhenMessageTemplateIsValid_SendsNotification()
    {
        // Arrange
        var input = new SendNotificationInput(
            NodeId: "test-node-4",
            MessageTemplate: "Test notification message"
        );

        // Act
        var act = async () => await _activities.SendNotificationAsync(input);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendNotificationAsync_WhenMessageTemplateIsEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        var input = new SendNotificationInput(
            NodeId: "test-node-5",
            MessageTemplate: null
        );

        // Act
        var act = async () => await _activities.SendNotificationAsync(input);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*MessageTemplate is required*");
    }

    [Fact]
    public async Task SendNotificationAsync_WhenMessageTemplateIsWhitespace_ThrowsInvalidOperationException()
    {
        // Arrange
        var input = new SendNotificationInput(
            NodeId: "test-node-6",
            MessageTemplate: "   "
        );

        // Act
        var act = async () => await _activities.SendNotificationAsync(input);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*MessageTemplate is required*");
    }
    #endregion
}
