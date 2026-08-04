using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration;

namespace TemporalDDD.Domain.Tests.WorkflowOrchestration;

public class SmartEnumTests
{
    #region Permission Tests
    [Fact]
    public void Permission_FromValue_WithValidValue_ReturnsCorrectEnum()
    {
        // ACT
        var result1 = Permission.FromValue(10);
        var result2 = Permission.FromValue(11);
        var result3 = Permission.FromValue(12);
        var result4 = Permission.FromValue(20);
        var result5 = Permission.FromValue(21);

        // ASSERT
        result1.IsSuccess.Should().BeTrue();
        result1.Value.Should().Be(Permission.CreateWorkflow);
        result2.IsSuccess.Should().BeTrue();
        result2.Value.Should().Be(Permission.EditWorkflow);
        result3.IsSuccess.Should().BeTrue();
        result3.Value.Should().Be(Permission.ApproveWorkflow);
        result4.IsSuccess.Should().BeTrue();
        result4.Value.Should().Be(Permission.ConfigureApiNode);
        result5.IsSuccess.Should().BeTrue();
        result5.Value.Should().Be(Permission.ConfigureQueryNode);
    }

    [Fact]
    public void Permission_FromValue_WithInvalidValue_ReturnsFailure()
    {
        // ACT
        var result = Permission.FromValue(99);

        // ASSERT
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid Permission value");
    }

    [Fact]
    public void Permission_FromCode_WithValidCode_ReturnsCorrectEnum()
    {
        // ACT
        var result1 = Permission.FromCode("workflow:create");
        var result2 = Permission.FromCode("workflow:edit");
        var result3 = Permission.FromCode("workflow:approve");
        var result4 = Permission.FromCode("node:api:configure");
        var result5 = Permission.FromCode("node:query:configure");

        // ASSERT
        result1.IsSuccess.Should().BeTrue();
        result1.Value.Should().Be(Permission.CreateWorkflow);
        result2.IsSuccess.Should().BeTrue();
        result2.Value.Should().Be(Permission.EditWorkflow);
        result3.IsSuccess.Should().BeTrue();
        result3.Value.Should().Be(Permission.ApproveWorkflow);
        result4.IsSuccess.Should().BeTrue();
        result4.Value.Should().Be(Permission.ConfigureApiNode);
        result5.IsSuccess.Should().BeTrue();
        result5.Value.Should().Be(Permission.ConfigureQueryNode);
    }

    [Fact]
    public void Permission_FromCode_WithInvalidCode_ReturnsFailure()
    {
        // ACT
        var result = Permission.FromCode("INVALID_CODE");

        // ASSERT
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid Permission code");
    }

    [Fact]
    public void Permission_ImplicitOperator_ReturnsCorrectIntValue()
    {
        // ACT
        int value1 = Permission.CreateWorkflow;
        int value2 = Permission.EditWorkflow;
        int value3 = Permission.ApproveWorkflow;

        // ASSERT
        value1.Should().Be(10);
        value2.Should().Be(11);
        value3.Should().Be(12);
    }

    [Fact]
    public void Permission_ToString_ReturnsCorrectName()
    {
        // ACT
        var name1 = Permission.CreateWorkflow.ToString();
        var name2 = Permission.EditWorkflow.ToString();
        var name3 = Permission.ApproveWorkflow.ToString();

        // ASSERT
        name1.Should().Be("Create Workflow");
        name2.Should().Be("Edit Workflow");
        name3.Should().Be("Approve Workflow");
    }
    #endregion

    #region WorkflowStatus Tests
    [Fact]
    public void WorkflowStatus_FromValue_WithValidValue_ReturnsCorrectEnum()
    {
        // ACT
        var result1 = WorkflowStatus.FromValue(0);
        var result2 = WorkflowStatus.FromValue(1);
        var result3 = WorkflowStatus.FromValue(2);
        var result4 = WorkflowStatus.FromValue(3);

        // ASSERT
        result1.IsSuccess.Should().BeTrue();
        result1.Value.Should().Be(WorkflowStatus.Draft);
        result2.IsSuccess.Should().BeTrue();
        result2.Value.Should().Be(WorkflowStatus.PendingReview);
        result3.IsSuccess.Should().BeTrue();
        result3.Value.Should().Be(WorkflowStatus.Approved);
        result4.IsSuccess.Should().BeTrue();
        result4.Value.Should().Be(WorkflowStatus.Rejected);
    }

    [Fact]
    public void WorkflowStatus_FromValue_WithInvalidValue_ReturnsFailure()
    {
        // ACT
        var result = WorkflowStatus.FromValue(99);

        // ASSERT
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid WorkflowStatus value");
    }

    [Fact]
    public void WorkflowStatus_ImplicitOperator_ReturnsCorrectIntValue()
    {
        // ACT
        int value1 = WorkflowStatus.Draft;
        int value2 = WorkflowStatus.PendingReview;
        int value3 = WorkflowStatus.Approved;
        int value4 = WorkflowStatus.Rejected;

        // ASSERT
        value1.Should().Be(0);
        value2.Should().Be(1);
        value3.Should().Be(2);
        value4.Should().Be(3);
    }

    [Fact]
    public void WorkflowStatus_ToString_ReturnsCorrectName()
    {
        // ACT
        var name1 = WorkflowStatus.Draft.ToString();
        var name2 = WorkflowStatus.PendingReview.ToString();
        var name3 = WorkflowStatus.Approved.ToString();
        var name4 = WorkflowStatus.Rejected.ToString();

        // ASSERT
        name1.Should().Be("Draft");
        name2.Should().Be("PendingReview");
        name3.Should().Be("Approved");
        name4.Should().Be("Rejected");
    }
    #endregion

    #region NodeType Tests
    [Fact]
    public void NodeType_FromValue_WithValidValue_ReturnsCorrectEnum()
    {
        // ACT
        var result1 = NodeType.FromValue(1);
        var result2 = NodeType.FromValue(2);
        var result3 = NodeType.FromValue(3);
        var result4 = NodeType.FromValue(4);
        var result5 = NodeType.FromValue(5);
        var result6 = NodeType.FromValue(6);

        // ASSERT
        result1.IsSuccess.Should().BeTrue();
        result1.Value.Should().Be(NodeType.Api);
        result2.IsSuccess.Should().BeTrue();
        result2.Value.Should().Be(NodeType.Notification);
        result3.IsSuccess.Should().BeTrue();
        result3.Value.Should().Be(NodeType.HumanTask);
        result4.IsSuccess.Should().BeTrue();
        result4.Value.Should().Be(NodeType.Delay);
        result5.IsSuccess.Should().BeTrue();
        result5.Value.Should().Be(NodeType.Decision);
        result6.IsSuccess.Should().BeTrue();
        result6.Value.Should().Be(NodeType.DataTransformation);
    }

    [Fact]
    public void NodeType_FromValue_WithInvalidValue_ReturnsFailure()
    {
        // ACT
        var result = NodeType.FromValue(99);

        // ASSERT
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid NodeType value");
    }

    [Fact]
    public void NodeType_ImplicitOperator_ReturnsCorrectIntValue()
    {
        // ACT
        int value1 = NodeType.Api;
        int value2 = NodeType.Notification;
        int value3 = NodeType.HumanTask;
        int value4 = NodeType.Delay;
        int value5 = NodeType.Decision;
        int value6 = NodeType.DataTransformation;

        // ASSERT
        value1.Should().Be(1);
        value2.Should().Be(2);
        value3.Should().Be(3);
        value4.Should().Be(4);
        value5.Should().Be(5);
        value6.Should().Be(6);
    }

    [Fact]
    public void NodeType_ToString_ReturnsCorrectName()
    {
        // ACT
        var name1 = NodeType.Api.ToString();
        var name2 = NodeType.Notification.ToString();
        var name3 = NodeType.HumanTask.ToString();
        var name4 = NodeType.Delay.ToString();
        var name5 = NodeType.Decision.ToString();
        var name6 = NodeType.DataTransformation.ToString();

        // ASSERT
        name1.Should().Be("Api");
        name2.Should().Be("Notification");
        name3.Should().Be("HumanTask");
        name4.Should().Be("Delay");
        name5.Should().Be("Decision");
        name6.Should().Be("DataTransformation");
    }
    #endregion
}
