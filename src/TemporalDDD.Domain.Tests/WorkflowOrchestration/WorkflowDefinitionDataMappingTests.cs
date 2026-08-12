using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration.Nodes;
using TemporalDDD.Domain.Tests.Builders;
using TemporalDDD.Domain.WorkflowOrchestration.Events;
using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;

namespace TemporalDDD.Domain.Tests.WorkflowOrchestration;

public class WorkflowDefinitionDataMappingTests
{
    #region Valid Mapping Tests
    [Fact]
    public void Approve_WhenDataMappedCorrectlyFromAncestor_ReturnsSuccess()
    {
        // ARRANGE
        var builder = new WorkflowBuilder();
        
        // Add workflow input that Start node will output
        builder.WithWorkflowInput(new NodeOutputDefinition("UserId", WorkflowDataType.Primitive.String));
        
        // Create API node (now has fixed output: ApiResponse as JsonDocument)
        builder.WithApiNode("ApiNode", out var apiNodeId);
        
        // Create HumanTask node that can accept the JsonDocument output
        builder.WithHumanTaskNode("HumanTaskNode", out var humanTaskNodeId,
            inputs: new[] { new NodeInputDefinition("ApiData", WorkflowDataType.Primitive.Json, true) });
        
        // Build the DAG: Start -> Api -> HumanTask -> End
        builder.WithTransition("Start", "ApiNode");
        builder.WithTransition("ApiNode", "HumanTaskNode");
        builder.WithTransition("HumanTaskNode", "End");
        
        // Map API output to HumanTask input
        builder.WithDataMapping("HumanTaskNode", "ApiData", "ApiNode", "ApiResponse");
        
        var workflow = builder.ReadyForApproval().Build();
        var reviewerId = UserId.New();

        // ACT
        workflow.Approve(reviewerId);

        // ASSERT
        workflow.Status.Should().Be(WorkflowStatus.Approved);
        workflow.DomainEvents.Should().ContainSingle(e => e is WorkflowApproved);
    }
    #endregion

    #region Scope Violation Tests
    [Fact]
    public void Approve_WhenDataMappedFromNonAncestorNode_ReturnsFailure()
    {
        // ARRANGE
        var builder = new WorkflowBuilder();
        
        // Create two parallel branches: Start -> NodeA -> End and Start -> NodeB
        // API nodes now have fixed output: ApiResponse as JsonDocument
        builder.WithApiNode("NodeA", out var nodeAId);
        builder.WithApiNode("NodeB", out var nodeBId);
        
        // Build DAG: Start -> NodeA -> End and Start -> NodeB -> End
        builder.WithTransition("Start", "NodeA");
        builder.WithTransition("NodeA", "End");
        builder.WithTransition("Start", "NodeB");
        builder.WithTransition("NodeB", "End");
        
        // Try to map NodeB's output to NodeA's input (invalid - NodeB is not an ancestor of NodeA)
        builder.WithDataMapping("NodeA", "EndpointUrl", "NodeB", "ApiResponse");
        
        var workflow = builder.ReadyForApproval().Build();
        var reviewerId = UserId.New();

        // ACT
        var action = () => workflow.Approve(reviewerId);

        // ASSERT
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*is not an upstream ancestor*");
    }
    #endregion

    #region Semantic Type Mismatch Tests
    [Fact]
    public void Approve_WhenDataMappedWithIncompatibleTypes_ReturnsFailure()
    {
        // ARRANGE
        var builder = new WorkflowBuilder();
        
        // Create API node (now has fixed output: ApiResponse as JsonDocument)
        builder.WithApiNode("ApiNode", out var apiNodeId);
        
        // Create HumanTask node that requires a String input
        builder.WithHumanTaskNode("HumanTaskNode", out var humanTaskNodeId,
            inputs: new[] { new NodeInputDefinition("StringData", WorkflowDataType.Primitive.String, true) });
        
        // Build DAG: Start -> Api -> HumanTask -> End
        builder.WithTransition("Start", "ApiNode");
        builder.WithTransition("ApiNode", "HumanTaskNode");
        builder.WithTransition("HumanTaskNode", "End");
        
        // Map Json output to String input (type mismatch)
        builder.WithDataMapping("HumanTaskNode", "StringData", "ApiNode", "ApiResponse");
        
        var workflow = builder.ReadyForApproval().Build();
        var reviewerId = UserId.New();

        // ACT
        var action = () => workflow.Approve(reviewerId);

        // ASSERT
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Type mismatch*");
    }
    #endregion

    #region Mapped Technical Property Validation Tests
    [Fact]
    public void Approve_WhenMappedPropertyPointsToInvalidAncestor_ReturnsFailure()
    {
        // ARRANGE
        var builder = new WorkflowBuilder();
        
        // Add workflow input that Start node will output
        builder.WithWorkflowInput(new NodeOutputDefinition("UserId", WorkflowDataType.Primitive.String));
        
        // Create API node with Mapped EndpointUrl pointing to non-ancestor
        builder.WithApiNode("ApiNode", out var apiNodeId, skipConfiguration: true);
        
        // Create a downstream node that's not an ancestor
        builder.WithApiNode("DownstreamNode", out var downstreamNodeId);
        
        // Build DAG: Start -> ApiNode -> DownstreamNode -> End
        builder.WithTransition("Start", "ApiNode");
        builder.WithTransition("ApiNode", "DownstreamNode");
        builder.WithTransition("DownstreamNode", "End");
        
        // Configure ApiNode with Mapped EndpointUrl pointing to DownstreamNode (invalid - not an ancestor)
        var apiNode = builder.Build().Nodes.OfType<ApiWorkflowNode>().First(n => n.Id == apiNodeId);
        var retryPolicy = RetryPolicy.Create(3, 2).Value!;
        var contractMapping = ContractMapping.Create(true, null, null, null).Value!;
        apiNode.SetTechnicalInput(ApiWorkflowNode.EndpointUrlKey, new InputValueSource.Mapped(new VariableReference(downstreamNodeId, "ApiResponse")));
        apiNode.SetTechnicalInput(ApiWorkflowNode.AuthTokenKey, new InputValueSource.Fixed("token"));
        apiNode.ConfigureValueObjects(retryPolicy, contractMapping);
        
        var workflow = builder.ReadyForApproval().Build();
        var reviewerId = UserId.New();

        // ACT
        var action = () => workflow.Approve(reviewerId);

        // ASSERT
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*is not an upstream ancestor*");
    }

    [Fact]
    public void Approve_WhenMappedPropertyPointsToWrongDataType_ReturnsFailure()
    {
        // ARRANGE
        var builder = new WorkflowBuilder();
        
        // Create API node (has fixed output: ApiResponse as JsonDocument)
        builder.WithApiNode("ApiNode", out var apiNodeId);
        
        // Create Notification node with Mapped MessageTemplate pointing to Json output (invalid - must be String)
        builder.WithNotificationNode("NotificationNode", out var notificationNodeId, skipConfiguration: true);
        
        // Build DAG: Start -> Api -> Notification -> End
        builder.WithTransition("Start", "ApiNode");
        builder.WithTransition("ApiNode", "NotificationNode");
        builder.WithTransition("NotificationNode", "End");
        
        // Configure NotificationNode with Mapped MessageTemplate pointing to Json output
        var notificationNode = builder.Build().Nodes.OfType<NotificationWorkflowNode>().First(n => n.Id == notificationNodeId);
        notificationNode.SetTechnicalInput(NotificationWorkflowNode.MessageTemplateKey, new InputValueSource.Mapped(new VariableReference(apiNodeId, "ApiResponse")));
        
        var workflow = builder.ReadyForApproval().Build();
        var reviewerId = UserId.New();

        // ACT
        var action = () => workflow.Approve(reviewerId);

        // ASSERT
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Technical properties require String type*");
    }

    [Fact]
    public void Approve_WhenMappedPropertyPointsToValidStringAncestor_ReturnsSuccess()
    {
        // ARRANGE
        var builder = new WorkflowBuilder();
        
        // Add workflow input that Start node will output as String
        builder.WithWorkflowInput(new NodeOutputDefinition("ApiUrl", WorkflowDataType.Primitive.String));
        
        // Create Notification node with Mapped MessageTemplate pointing to Start's String output
        builder.WithNotificationNode("NotificationNode", out var notificationNodeId, skipConfiguration: true);
        
        // Build DAG: Start -> Notification -> End
        builder.WithTransition("Start", "NotificationNode");
        builder.WithTransition("NotificationNode", "End");
        
        // Configure NotificationNode with Mapped MessageTemplate pointing to Start's String output
        var notificationNode = builder.Build().Nodes.OfType<NotificationWorkflowNode>().First(n => n.Id == notificationNodeId);
        notificationNode.SetTechnicalInput(NotificationWorkflowNode.MessageTemplateKey, new InputValueSource.Mapped(new VariableReference(builder.Build().Nodes.OfType<StartWorkflowNode>().First().Id, "ApiUrl")));
        
        var workflow = builder.ReadyForApproval().Build();
        var reviewerId = UserId.New();

        // ACT
        workflow.Approve(reviewerId);

        // ASSERT
        workflow.Status.Should().Be(WorkflowStatus.Approved);
        workflow.DomainEvents.Should().ContainSingle(e => e is WorkflowApproved);
    }
    #endregion
}
