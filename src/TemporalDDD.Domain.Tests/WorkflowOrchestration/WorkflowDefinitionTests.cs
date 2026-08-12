using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration.Events;
using TemporalDDD.Domain.WorkflowOrchestration.Nodes;

namespace TemporalDDD.Domain.Tests.WorkflowOrchestration;

public class WorkflowDefinitionTests
{
    #region Create Tests
    [Fact]
    public void Create_WithValidParameters_SetsDraftStatus()
    {
        // ARRANGE
        var creatorId = UserId.New();
        var name = "Test Workflow";
        var initialJson = "{}";
        var publicId = WorkflowDefinitionPublicId.New();

        // ACT
        var workflow = WorkflowDefinition.Create(creatorId, name, initialJson, publicId);

        // ASSERT
        workflow.Status.Should().Be(WorkflowStatus.Draft);
        workflow.Name.Should().Be(name);
        workflow.FlowJson.Should().Be(initialJson);
        workflow.CreatorId.Should().Be(creatorId);
        workflow.PublicId.Should().Be(publicId);
    }
    #endregion

    #region UpdateFlowJson Tests
    [Fact]
    public void UpdateFlowJson_WhenDraft_UpdatesJson()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);
        var newJson = "{\"updated\": true}";

        // ACT
        workflow.UpdateFlowJson(newJson);

        // ASSERT
        workflow.FlowJson.Should().Be(newJson);
    }

    [Fact]
    public void UpdateFlowJson_WhenRejected_UpdatesJson()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);
        workflow.SubmitForReview();
        workflow.Reject(UserId.New(), "Test rejection");

        var newJson = "{\"updated\": true}";

        // ACT
        workflow.UpdateFlowJson(newJson);

        // ASSERT
        workflow.FlowJson.Should().Be(newJson);
    }

    [Fact]
    public void UpdateFlowJson_WhenNotDraftOrRejected_ThrowsException()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);
        workflow.SubmitForReview();

        // ACT
        var action = () => workflow.UpdateFlowJson("{\"updated\": true}");

        // ASSERT
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot update flow JSON when status is not Draft or Rejected");
    }
    #endregion

    #region SubmitForReview Tests
    [Fact]
    public void SubmitForReview_WhenDraft_ChangesToPendingReviewAndRaisesEvent()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);

        // ACT
        workflow.SubmitForReview();

        // ASSERT
        workflow.Status.Should().Be(WorkflowStatus.PendingReview);
        workflow.DomainEvents.Should().ContainSingle(e => e is WorkflowSubmittedForReview);
    }

    [Fact]
    public void SubmitForReview_WhenRejected_ChangesToPendingReviewAndRaisesEvent()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);
        workflow.SubmitForReview();
        workflow.Reject(UserId.New(), "Test rejection");

        // ACT
        workflow.SubmitForReview();

        // ASSERT
        workflow.Status.Should().Be(WorkflowStatus.PendingReview);
        workflow.DomainEvents.Should().Contain(e => e is WorkflowSubmittedForReview);
    }

    [Fact]
    public void SubmitForReview_WhenNotDraftOrRejected_ThrowsException()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);
        workflow.SubmitForReview();

        // ACT
        var action = () => workflow.SubmitForReview();

        // ASSERT
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot submit for review when status is not Draft or Rejected");
    }
    #endregion

    #region Approve Tests
    [Fact]
    public void Approve_WhenPendingReviewAndAllNodesConfigured_ChangesToApprovedAndRaisesEvent()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);
        workflow.AddApiNodeStub("API Node", "Business notes");
        var apiNode = workflow.Nodes.OfType<ApiWorkflowNode>().First();
        apiNode.SetTechnicalInput(ApiWorkflowNode.EndpointUrlKey, new InputValueSource.Fixed("https://api.example.com"));
        apiNode.SetTechnicalInput(ApiWorkflowNode.AuthTokenKey, new InputValueSource.Fixed("token"));
        apiNode.ConfigureValueObjects(
            TemporalDDD.Domain.WorkflowOrchestration.ValueObjects.RetryPolicy.Create(3, 2).Value!,
            TemporalDDD.Domain.WorkflowOrchestration.ValueObjects.ContractMapping.Create(true, null, null, null).Value!
        );
        
        // Add transitions to make topology valid (Start -> API -> End)
        var startNode = workflow.Nodes.OfType<StartWorkflowNode>().First();
        var endNode = workflow.Nodes.OfType<EndWorkflowNode>().First();
        var transitions = new List<WorkflowTransition>
        {
            new WorkflowTransition(startNode.Id, apiNode.Id),
            new WorkflowTransition(apiNode.Id, endNode.Id)
        };
        workflow.UpdateNodes(workflow.Nodes.ToList(), transitions, null);
        
        workflow.SubmitForReview();
        var reviewerId = UserId.New();

        // ACT
        workflow.Approve(reviewerId);

        // ASSERT
        workflow.Status.Should().Be(WorkflowStatus.Approved);
        workflow.DomainEvents.Should().ContainSingle(e => e is WorkflowApproved);
    }

    [Fact]
    public void Approve_WhenNotPendingReview_ThrowsException()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);

        // ACT
        var action = () => workflow.Approve(UserId.New());

        // ASSERT
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot approve workflow when status is not PendingReview");
    }

    [Fact]
    public void Approve_WhenNodesNotConfigured_ThrowsException()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);
        workflow.AddApiNodeStub("API Node", "Business notes");
        workflow.SubmitForReview();

        // ACT
        var action = () => workflow.Approve(UserId.New());

        // ASSERT
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot approve workflow: One or more nodes are missing technical configuration.");
    }
    #endregion

    #region Reject Tests
    [Fact]
    public void Reject_WhenPendingReview_ChangesToRejectedAndRaisesEvent()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);
        workflow.SubmitForReview();
        var reviewerId = UserId.New();
        var reason = "Test rejection";

        // ACT
        workflow.Reject(reviewerId, reason);

        // ASSERT
        workflow.Status.Should().Be(WorkflowStatus.Rejected);
        workflow.DomainEvents.Should().ContainSingle(e => e is WorkflowRejected);
    }

    [Fact]
    public void Reject_WhenNotPendingReview_ThrowsException()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);

        // ACT
        var action = () => workflow.Reject(UserId.New(), "Test rejection");

        // ASSERT
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot reject workflow when status is not PendingReview");
    }
    #endregion

    #region Node Management Tests
    [Fact]
    public void AddApiNodeStub_AddsNodeToCollection()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);

        // ACT
        workflow.AddApiNodeStub("API Node", "Business notes");

        // ASSERT
        workflow.Nodes.Should().ContainSingle(n => n.Name == "API Node");
        workflow.Nodes.OfType<ApiWorkflowNode>().Should().ContainSingle();
    }

    [Fact]
    public void AddNotificationNodeStub_AddsNodeToCollection()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);

        // ACT
        workflow.AddNotificationNodeStub("Notification Node", "Business notes");

        // ASSERT
        workflow.Nodes.Should().ContainSingle(n => n.Name == "Notification Node");
        workflow.Nodes.OfType<NotificationWorkflowNode>().Should().ContainSingle();
    }

    [Fact]
    public void Nodes_IsReadOnlyCollection()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);

        // ACT
        var nodes = workflow.Nodes;

        // ASSERT
        nodes.Should().BeAssignableTo<System.Collections.Generic.IReadOnlyCollection<WorkflowNode>>();
    }
    #endregion

    #region UpdateNodes Tests
    [Fact]
    public void UpdateNodes_WithFlowJson_UpdatesFlowJsonProperty()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);
        var newFlowJson = "{\"nodes\":[],\"edges\":[]}";
        var nodes = workflow.Nodes.ToList();
        var transitions = workflow.Transitions.ToList();

        // ACT
        workflow.UpdateNodes(nodes, transitions, newFlowJson);

        // ASSERT
        workflow.FlowJson.Should().Be(newFlowJson);
    }

    [Fact]
    public void UpdateNodes_WithNullFlowJson_DoesNotChangeFlowJsonProperty()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var initialJson = "{\"initial\": true}";
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", initialJson, publicId);
        var nodes = workflow.Nodes.ToList();
        var transitions = workflow.Transitions.ToList();

        // ACT
        workflow.UpdateNodes(nodes, transitions, null);

        // ASSERT
        workflow.FlowJson.Should().Be(initialJson);
    }

    [Fact]
    public void UpdateNodes_ReplacesExistingNodesAndTransitions()
    {
        // ARRANGE
        var publicId = WorkflowDefinitionPublicId.New();
        var workflow = WorkflowDefinition.Create(UserId.New(), "Test", "{}", publicId);
        var initialNodeCount = workflow.Nodes.Count;
        var initialTransitionCount = workflow.Transitions.Count;

        var newNodes = new List<WorkflowNode>
        {
            StartWorkflowNode.CreateStub("New Start", "New start node"),
            EndWorkflowNode.CreateStub("New End", "New end node")
        };
        var newTransitions = new List<WorkflowTransition>
        {
            new WorkflowTransition(newNodes[0].Id, newNodes[1].Id)
        };

        // ACT
        workflow.UpdateNodes(newNodes, newTransitions, null);

        // ASSERT
        workflow.Nodes.Should().HaveCount(2);
        workflow.Transitions.Should().HaveCount(1);
        workflow.Nodes.Should().NotContain(n => n.Name == "Start");
        workflow.Nodes.Should().Contain(n => n.Name == "New Start");
    }
    #endregion
}
