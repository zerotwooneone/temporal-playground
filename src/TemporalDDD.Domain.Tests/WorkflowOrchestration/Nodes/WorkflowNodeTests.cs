using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration.Nodes;
using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

namespace TemporalDDD.Domain.Tests.WorkflowOrchestration.Nodes;

public class WorkflowNodeTests
{
    #region ApiWorkflowNode Tests
    [Fact]
    public void ApiWorkflowNode_WhenAddedViaAggregate_HasCorrectDefaults()
    {
        // ARRANGE
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}");

        // ACT
        workflow.AddApiNodeStub("API Node", "Business notes");

        // ASSERT
        var node = workflow.Nodes.OfType<ApiWorkflowNode>().First();
        node.Name.Should().Be("API Node");
        node.BusinessNotes.Should().Be("Business notes");
        node.Type.Should().Be(NodeType.Api);
        node.IsConfigured.Should().BeFalse();
        node.Id.Should().NotBeNull();
    }

    [Fact]
    public void ApiWorkflowNode_ValidateConfiguration_WithAllRequiredFields_SetsIsConfiguredToTrue()
    {
        // ARRANGE
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}");
        workflow.AddApiNodeStub("API Node", "Business notes");
        var node = workflow.Nodes.OfType<ApiWorkflowNode>().First();
        var retryPolicy = RetryPolicy.Create(3, 2).Value!;
        var mapping = ContractMapping.Create(true, null, null, null).Value!;

        // ACT
        node.ConfigureTechnicalDetails("https://api.example.com", "token", retryPolicy, mapping);

        // ASSERT
        node.IsConfigured.Should().BeTrue();
    }

    [Fact]
    public void ApiWorkflowNode_ValidateConfiguration_WithMissingEndpointUrl_SetsIsConfiguredToFalse()
    {
        // ARRANGE
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}");
        workflow.AddApiNodeStub("API Node", "Business notes");
        var node = workflow.Nodes.OfType<ApiWorkflowNode>().First();
        var retryPolicy = RetryPolicy.Create(3, 2).Value!;
        var mapping = ContractMapping.Create(true, null, null, null).Value!;

        // ACT
        node.ConfigureTechnicalDetails(null, "token", retryPolicy, mapping);

        // ASSERT
        node.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public void ApiWorkflowNode_ValidateConfiguration_WithMissingRetryPolicy_SetsIsConfiguredToFalse()
    {
        // ARRANGE
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}");
        workflow.AddApiNodeStub("API Node", "Business notes");
        var node = workflow.Nodes.OfType<ApiWorkflowNode>().First();
        var mapping = ContractMapping.Create(true, null, null, null).Value!;

        // ACT
        node.ConfigureTechnicalDetails("https://api.example.com", "token", null, mapping);

        // ASSERT
        node.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public void ApiWorkflowNode_ValidateConfiguration_WithMissingContractMapping_SetsIsConfiguredToFalse()
    {
        // ARRANGE
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}");
        workflow.AddApiNodeStub("API Node", "Business notes");
        var node = workflow.Nodes.OfType<ApiWorkflowNode>().First();
        var retryPolicy = RetryPolicy.Create(3, 2).Value!;

        // ACT
        node.ConfigureTechnicalDetails("https://api.example.com", "token", retryPolicy, null);

        // ASSERT
        node.IsConfigured.Should().BeFalse();
    }
    #endregion

    #region NotificationWorkflowNode Tests
    [Fact]
    public void NotificationWorkflowNode_WhenAddedViaAggregate_HasCorrectDefaults()
    {
        // ARRANGE
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}");

        // ACT
        workflow.AddNotificationNodeStub("Notification Node", "Business notes");

        // ASSERT
        var node = workflow.Nodes.OfType<NotificationWorkflowNode>().First();
        node.Name.Should().Be("Notification Node");
        node.BusinessNotes.Should().Be("Business notes");
        node.Type.Should().Be(NodeType.Notification);
        node.IsConfigured.Should().BeFalse();
        node.Id.Should().NotBeNull();
    }

    [Fact]
    public void NotificationWorkflowNode_ValidateConfiguration_WithValidTemplate_SetsIsConfiguredToTrue()
    {
        // ARRANGE
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}");
        workflow.AddNotificationNodeStub("Notification Node", "Business notes");
        var node = workflow.Nodes.OfType<NotificationWorkflowNode>().First();

        // ACT
        node.ConfigureTechnicalDetails("Hello {name}");

        // ASSERT
        node.IsConfigured.Should().BeTrue();
    }

    [Fact]
    public void NotificationWorkflowNode_ValidateConfiguration_WithNullTemplate_SetsIsConfiguredToFalse()
    {
        // ARRANGE
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}");
        workflow.AddNotificationNodeStub("Notification Node", "Business notes");
        var node = workflow.Nodes.OfType<NotificationWorkflowNode>().First();

        // ACT
        node.ConfigureTechnicalDetails(null!);

        // ASSERT
        node.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public void NotificationWorkflowNode_ValidateConfiguration_WithWhitespaceTemplate_SetsIsConfiguredToFalse()
    {
        // ARRANGE
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}");
        workflow.AddNotificationNodeStub("Notification Node", "Business notes");
        var node = workflow.Nodes.OfType<NotificationWorkflowNode>().First();

        // ACT
        node.ConfigureTechnicalDetails("   ");

        // ASSERT
        node.IsConfigured.Should().BeFalse();
    }
    #endregion
}
