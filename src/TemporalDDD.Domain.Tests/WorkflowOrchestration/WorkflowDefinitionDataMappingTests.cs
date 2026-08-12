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
}
