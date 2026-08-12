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
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);

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
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);
        workflow.AddApiNodeStub("API Node", "Business notes");
        var node = workflow.Nodes.OfType<ApiWorkflowNode>().First();
        var retryPolicy = RetryPolicy.Create(3, 2).Value!;
        var mapping = ContractMapping.Create(true, null, null, null).Value!;

        // ACT
        node.SetTechnicalInput("EndpointUrl", new InputValueSource.Fixed("https://api.example.com"));
        node.SetTechnicalInput("AuthToken", new InputValueSource.Fixed("token"));
        node.ConfigureValueObjects(retryPolicy, mapping);

        // ASSERT
        node.IsConfigured.Should().BeTrue();
    }

    [Fact]
    public void ApiWorkflowNode_ValidateConfiguration_WithMissingEndpointUrl_SetsIsConfiguredToFalse()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);
        workflow.AddApiNodeStub("API Node", "Business notes");
        var node = workflow.Nodes.OfType<ApiWorkflowNode>().First();
        var retryPolicy = RetryPolicy.Create(3, 2).Value!;
        var mapping = ContractMapping.Create(true, null, null, null).Value!;

        // ACT
        node.SetTechnicalInput("AuthToken", new InputValueSource.Fixed("token"));
        node.ConfigureValueObjects(retryPolicy, mapping);

        // ASSERT
        node.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public void ApiWorkflowNode_ValidateConfiguration_WithMissingRetryPolicy_SetsIsConfiguredToFalse()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);
        workflow.AddApiNodeStub("API Node", "Business notes");
        var node = workflow.Nodes.OfType<ApiWorkflowNode>().First();
        var mapping = ContractMapping.Create(true, null, null, null).Value!;

        // ACT
        node.SetTechnicalInput("EndpointUrl", new InputValueSource.Fixed("https://api.example.com"));
        node.SetTechnicalInput("AuthToken", new InputValueSource.Fixed("token"));
        node.ConfigureValueObjects(null, mapping);

        // ASSERT
        node.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public void ApiWorkflowNode_ValidateConfiguration_WithMissingContractMapping_SetsIsConfiguredToFalse()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);
        workflow.AddApiNodeStub("API Node", "Business notes");
        var node = workflow.Nodes.OfType<ApiWorkflowNode>().First();
        var retryPolicy = RetryPolicy.Create(3, 2).Value!;

        // ACT
        node.SetTechnicalInput("EndpointUrl", new InputValueSource.Fixed("https://api.example.com"));
        node.SetTechnicalInput("AuthToken", new InputValueSource.Fixed("token"));
        node.ConfigureValueObjects(retryPolicy, null);

        // ASSERT
        node.IsConfigured.Should().BeFalse();
    }
    #endregion

    #region NotificationWorkflowNode Tests
    [Fact]
    public void NotificationWorkflowNode_WhenAddedViaAggregate_HasCorrectDefaults()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);

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
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);
        workflow.AddNotificationNodeStub("Notification Node", "Business notes");
        var node = workflow.Nodes.OfType<NotificationWorkflowNode>().First();

        // ACT
        node.SetTechnicalInput("MessageTemplate", new InputValueSource.Fixed("Hello {name}"));

        // ASSERT
        node.IsConfigured.Should().BeTrue();
    }

    [Fact]
    public void NotificationWorkflowNode_ValidateConfiguration_WithNullTemplate_SetsIsConfiguredToFalse()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);
        workflow.AddNotificationNodeStub("Notification Node", "Business notes");
        var node = workflow.Nodes.OfType<NotificationWorkflowNode>().First();

        // ACT
        // Don't set any technical input

        // ASSERT
        node.IsConfigured.Should().BeFalse();
    }

    [Fact]
    public void NotificationWorkflowNode_ValidateConfiguration_WithWhitespaceTemplate_SetsIsConfiguredToFalse()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);
        workflow.AddNotificationNodeStub("Notification Node", "Business notes");
        var node = workflow.Nodes.OfType<NotificationWorkflowNode>().First();

        // ACT
        node.SetTechnicalInput("MessageTemplate", new InputValueSource.Fixed("   "));

        // ASSERT
        node.IsConfigured.Should().BeFalse();
    }
    #endregion
}
