using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration.Nodes;
using TemporalDDD.Domain.Tests.Builders;
using TemporalDDD.Domain.WorkflowOrchestration.Events;

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
        
        // Create API node that outputs a String result
        builder.WithApiNode("ApiNode", out var apiNodeId, 
            outputs: new[] { new NodeOutputDefinition("ApiResult", WorkflowDataType.Primitive.String) });
        
        // Create Notification node
        builder.WithNotificationNode("NotificationNode", out var notificationNodeId);
        
        // Build the DAG: Start -> Api -> Notification -> End
        builder.WithTransition("Start", "ApiNode");
        builder.WithTransition("ApiNode", "NotificationNode");
        builder.WithTransition("NotificationNode", "End");
        
        // Map API output to Notification input
        builder.WithDataMapping("NotificationNode", "MessageTemplate", "ApiNode", "ApiResult");
        
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
        builder.WithApiNode("NodeA", out var nodeAId, 
            outputs: new[] { new NodeOutputDefinition("OutputA", WorkflowDataType.Primitive.String) });
        builder.WithApiNode("NodeB", out var nodeBId,
            outputs: new[] { new NodeOutputDefinition("OutputB", WorkflowDataType.Primitive.String) });
        
        // Build DAG: Start -> NodeA -> End and Start -> NodeB -> End
        builder.WithTransition("Start", "NodeA");
        builder.WithTransition("NodeA", "End");
        builder.WithTransition("Start", "NodeB");
        builder.WithTransition("NodeB", "End");
        
        // Try to map NodeB's output to NodeA's input (invalid - NodeB is not an ancestor of NodeA)
        builder.WithDataMapping("NodeA", "EndpointUrl", "NodeB", "OutputB");
        
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
        
        // Create API node that outputs a Number
        builder.WithApiNode("ApiNode", out var apiNodeId,
            outputs: new[] { new NodeOutputDefinition("ApiResult", WorkflowDataType.Primitive.Number) });
        
        // Create Notification node that requires a String
        builder.WithNotificationNode("NotificationNode", out var notificationNodeId);
        
        // Build DAG: Start -> Api -> Notification -> End
        builder.WithTransition("Start", "ApiNode");
        builder.WithTransition("ApiNode", "NotificationNode");
        builder.WithTransition("NotificationNode", "End");
        
        // Map Number output to String input (type mismatch)
        builder.WithDataMapping("NotificationNode", "MessageTemplate", "ApiNode", "ApiResult");
        
        var workflow = builder.ReadyForApproval().Build();
        var reviewerId = UserId.New();

        // ACT
        var action = () => workflow.Approve(reviewerId);

        // ASSERT
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Type mismatch*");
    }
    #endregion
}
