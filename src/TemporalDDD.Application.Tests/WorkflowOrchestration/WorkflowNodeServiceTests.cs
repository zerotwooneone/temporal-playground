using Moq;
using TemporalDDD.Application.WorkflowOrchestration;
using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration.Nodes;

namespace TemporalDDD.Application.Tests.WorkflowOrchestration;

public class WorkflowNodeServiceTests
{
    private static readonly string FixedWorkflowDefinitionId = "WFLId00000000-0000-0000-0000-000000000001";
    private static readonly string FixedPublicId = "WFL_00000000-0000-0000-0000-000000000002";
    private static readonly string FixedUserId = "USRId00000000-0000-0000-0000-000000000003";
    private static readonly string FixedNodeId1 = "WFNId00000000-0000-0000-0000-000000000001";
    private static readonly string FixedNodeId2 = "WFNId00000000-0000-0000-0000-000000000002";
    private static readonly string FixedNodeId3 = "WFNId00000000-0000-0000-0000-000000000003";
    private static readonly string FixedNodeId4 = "WFNId00000000-0000-0000-0000-000000000004";

    #region UpdateNodesAsync Tests
    [Fact]
    public async Task UpdateNodesAsync_WhenWorkflowNotFound_ThrowsInvalidOperationException()
    {
        // ARRANGE
        var mockRepository = new Mock<IWorkflowDefinitionRepository>();
        mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<WorkflowDefinitionId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkflowDefinition?)null);

        var service = new WorkflowNodeService(mockRepository.Object);
        var workflowDefinitionId = WorkflowDefinitionId.Create(FixedWorkflowDefinitionId).Value;
        var input = new UpdateWorkflowNodesInput(
            workflowDefinitionId.ToString(),
            "{}",
            new List<WorkflowNodeDto>(),
            new List<WorkflowTransitionDto>()
        );

        // ACT
        var action = async () => await service.UpdateNodesAsync(workflowDefinitionId, "{}", input);

        // ASSERT
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Workflow with WorkflowDefinitionId {workflowDefinitionId} not found");
    }

    [Fact]
    public async Task UpdateNodesAsync_WithValidInput_UpdatesWorkflowAndSaves()
    {
        // ARRANGE
        var mockRepository = new Mock<IWorkflowDefinitionRepository>();
        var workflowDefinitionId = WorkflowDefinitionId.Create(FixedWorkflowDefinitionId).Value;
        var publicId = WorkflowDefinitionPublicId.Create(FixedPublicId).Value;
        var workflow = WorkflowDefinition.Create(UserId.Create(FixedUserId).Value, "Test", "{}", publicId);

        mockRepository.Setup(r => r.GetByIdAsync(workflowDefinitionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflow);
        mockRepository.Setup(r => r.SaveAsync(It.IsAny<WorkflowDefinition>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new WorkflowNodeService(mockRepository.Object);

        var nodeDtos = new List<WorkflowNodeDto>
        {
            new WorkflowNodeDto(
                Id: FixedNodeId1,
                NodeType: 1,
                Name: "API Node",
                BusinessNotes: "Notes",
                IsConfigured: true,
                EndpointUrl: "https://api.example.com",
                AuthToken: "token",
                RetryPolicyMaxAttempts: 3,
                RetryPolicyBackoffCoefficient: 2,
                ContractMappingConvertXmlToJson: true,
                ContractMappingQueryParameters: null,
                ContractMappingRequestMapping: null,
                ContractMappingResponseMapping: null,
                MessageTemplate: null
            )
        };

        var transitionDtos = new List<WorkflowTransitionDto>
        {
            new WorkflowTransitionDto(SourceNodeId: FixedNodeId1, TargetNodeId: FixedNodeId2, SourcePort: "Default")
        };

        var input = new UpdateWorkflowNodesInput(
            workflowDefinitionId.ToString(),
            "{\"nodes\":[],\"edges\":[]}",
            nodeDtos,
            transitionDtos
        );

        // ACT
        await service.UpdateNodesAsync(workflowDefinitionId, "{\"nodes\":[],\"edges\":[]}", input);

        // ASSERT
        mockRepository.Verify(r => r.SaveAsync(workflow, It.IsAny<CancellationToken>()), Times.Once);
        workflow.FlowJson.Should().Be("{\"nodes\":[],\"edges\":[]}");
    }

    [Fact]
    public async Task UpdateNodesAsync_WithInvalidNodeType_ThrowsInvalidOperationException()
    {
        // ARRANGE
        var mockRepository = new Mock<IWorkflowDefinitionRepository>();
        var workflowDefinitionId = WorkflowDefinitionId.Create(FixedWorkflowDefinitionId).Value;
        var publicId = WorkflowDefinitionPublicId.Create(FixedPublicId).Value;
        var workflow = WorkflowDefinition.Create(UserId.Create(FixedUserId).Value, "Test", "{}", publicId);

        mockRepository.Setup(r => r.GetByIdAsync(workflowDefinitionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflow);

        var service = new WorkflowNodeService(mockRepository.Object);

        var nodeDtos = new List<WorkflowNodeDto>
        {
            new WorkflowNodeDto(
                Id: "node-1",
                NodeType: 999, // Invalid node type
                Name: "Invalid Node",
                BusinessNotes: null,
                IsConfigured: false,
                EndpointUrl: null,
                AuthToken: null,
                RetryPolicyMaxAttempts: null,
                RetryPolicyBackoffCoefficient: null,
                ContractMappingConvertXmlToJson: null,
                ContractMappingQueryParameters: null,
                ContractMappingRequestMapping: null,
                ContractMappingResponseMapping: null,
                MessageTemplate: null
            )
        };

        var input = new UpdateWorkflowNodesInput(
            workflowDefinitionId.ToString(),
            "{}",
            nodeDtos,
            new List<WorkflowTransitionDto>()
        );

        // ACT
        var action = async () => await service.UpdateNodesAsync(workflowDefinitionId, "{}", input);

        // ASSERT
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid NodeType for node node-1*");
    }

    [Fact]
    public async Task UpdateNodesAsync_WithInvalidNodeId_ThrowsInvalidOperationException()
    {
        // ARRANGE
        var mockRepository = new Mock<IWorkflowDefinitionRepository>();
        var workflowDefinitionId = WorkflowDefinitionId.Create(FixedWorkflowDefinitionId).Value;
        var publicId = WorkflowDefinitionPublicId.Create(FixedPublicId).Value;
        var workflow = WorkflowDefinition.Create(UserId.Create(FixedUserId).Value, "Test", "{}", publicId);

        mockRepository.Setup(r => r.GetByIdAsync(workflowDefinitionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflow);

        var service = new WorkflowNodeService(mockRepository.Object);

        var nodeDtos = new List<WorkflowNodeDto>
        {
            new WorkflowNodeDto(
                Id: "", // Invalid empty ID
                NodeType: 1,
                Name: "API Node",
                BusinessNotes: null,
                IsConfigured: false,
                EndpointUrl: null,
                AuthToken: null,
                RetryPolicyMaxAttempts: null,
                RetryPolicyBackoffCoefficient: null,
                ContractMappingConvertXmlToJson: null,
                ContractMappingQueryParameters: null,
                ContractMappingRequestMapping: null,
                ContractMappingResponseMapping: null,
                MessageTemplate: null
            )
        };

        var input = new UpdateWorkflowNodesInput(
            workflowDefinitionId.ToString(),
            "{}",
            nodeDtos,
            new List<WorkflowTransitionDto>()
        );

        // ACT
        var action = async () => await service.UpdateNodesAsync(workflowDefinitionId, "{}", input);

        // ASSERT
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid NodeId for node *");
    }

    [Fact]
    public async Task UpdateNodesAsync_WithInvalidTransitionSourceId_ThrowsInvalidOperationException()
    {
        // ARRANGE
        var mockRepository = new Mock<IWorkflowDefinitionRepository>();
        var workflowDefinitionId = WorkflowDefinitionId.Create(FixedWorkflowDefinitionId).Value;
        var publicId = WorkflowDefinitionPublicId.Create(FixedPublicId).Value;
        var workflow = WorkflowDefinition.Create(UserId.Create(FixedUserId).Value, "Test", "{}", publicId);

        mockRepository.Setup(r => r.GetByIdAsync(workflowDefinitionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflow);

        var service = new WorkflowNodeService(mockRepository.Object);

        var transitionDtos = new List<WorkflowTransitionDto>
        {
            new WorkflowTransitionDto(SourceNodeId: "", TargetNodeId: FixedNodeId1) // Invalid empty source ID
        };

        var input = new UpdateWorkflowNodesInput(
            workflowDefinitionId.ToString(),
            "{}",
            new List<WorkflowNodeDto>(),
            transitionDtos
        );

        // ACT
        var action = async () => await service.UpdateNodesAsync(workflowDefinitionId, "{}", input);

        // ASSERT
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid SourceNodeId*");
    }

    [Fact]
    public async Task UpdateNodesAsync_WithInvalidTransitionTargetId_ThrowsInvalidOperationException()
    {
        // ARRANGE
        var mockRepository = new Mock<IWorkflowDefinitionRepository>();
        var workflowDefinitionId = WorkflowDefinitionId.Create(FixedWorkflowDefinitionId).Value;
        var publicId = WorkflowDefinitionPublicId.Create(FixedPublicId).Value;
        var workflow = WorkflowDefinition.Create(UserId.Create(FixedUserId).Value, "Test", "{}", publicId);

        mockRepository.Setup(r => r.GetByIdAsync(workflowDefinitionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflow);

        var service = new WorkflowNodeService(mockRepository.Object);

        var transitionDtos = new List<WorkflowTransitionDto>
        {
            new WorkflowTransitionDto(SourceNodeId: FixedNodeId1, TargetNodeId: "") // Invalid empty target ID
        };

        var input = new UpdateWorkflowNodesInput(
            workflowDefinitionId.ToString(),
            "{}",
            new List<WorkflowNodeDto>(),
            transitionDtos
        );

        // ACT
        var action = async () => await service.UpdateNodesAsync(workflowDefinitionId, "{}", input);

        // ASSERT
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid TargetNodeId*");
    }

    [Fact]
    public async Task UpdateNodesAsync_MapsAllNodeTypesCorrectly()
    {
        // ARRANGE
        var mockRepository = new Mock<IWorkflowDefinitionRepository>();
        var workflowDefinitionId = WorkflowDefinitionId.Create(FixedWorkflowDefinitionId).Value;
        var publicId = WorkflowDefinitionPublicId.Create(FixedPublicId).Value;
        var workflow = WorkflowDefinition.Create(UserId.Create(FixedUserId).Value, "Test", "{}", publicId);

        mockRepository.Setup(r => r.GetByIdAsync(workflowDefinitionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflow);
        mockRepository.Setup(r => r.SaveAsync(It.IsAny<WorkflowDefinition>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new WorkflowNodeService(mockRepository.Object);

        var nodeDtos = new List<WorkflowNodeDto>
        {
            new WorkflowNodeDto(
                Id: FixedNodeId1,
                NodeType: 0, // Start
                Name: "Start",
                BusinessNotes: null,
                IsConfigured: true,
                EndpointUrl: null,
                AuthToken: null,
                RetryPolicyMaxAttempts: null,
                RetryPolicyBackoffCoefficient: null,
                ContractMappingConvertXmlToJson: null,
                ContractMappingQueryParameters: null,
                ContractMappingRequestMapping: null,
                ContractMappingResponseMapping: null,
                MessageTemplate: null
            ),
            new WorkflowNodeDto(
                Id: FixedNodeId2,
                NodeType: 1, // API
                Name: "API",
                BusinessNotes: null,
                IsConfigured: true,
                EndpointUrl: "https://api.example.com",
                AuthToken: "token",
                RetryPolicyMaxAttempts: 3,
                RetryPolicyBackoffCoefficient: 2,
                ContractMappingConvertXmlToJson: true,
                ContractMappingQueryParameters: null,
                ContractMappingRequestMapping: null,
                ContractMappingResponseMapping: null,
                MessageTemplate: null
            ),
            new WorkflowNodeDto(
                Id: FixedNodeId3,
                NodeType: 2, // Notification
                Name: "Notification",
                BusinessNotes: null,
                IsConfigured: true,
                EndpointUrl: null,
                AuthToken: null,
                RetryPolicyMaxAttempts: null,
                RetryPolicyBackoffCoefficient: null,
                ContractMappingConvertXmlToJson: null,
                ContractMappingQueryParameters: null,
                ContractMappingRequestMapping: null,
                ContractMappingResponseMapping: null,
                MessageTemplate: "Hello {name}"
            ),
            new WorkflowNodeDto(
                Id: FixedNodeId4,
                NodeType: 99, // End
                Name: "End",
                BusinessNotes: null,
                IsConfigured: true,
                EndpointUrl: null,
                AuthToken: null,
                RetryPolicyMaxAttempts: null,
                RetryPolicyBackoffCoefficient: null,
                ContractMappingConvertXmlToJson: null,
                ContractMappingQueryParameters: null,
                ContractMappingRequestMapping: null,
                ContractMappingResponseMapping: null,
                MessageTemplate: null
            )
        };

        var input = new UpdateWorkflowNodesInput(
            workflowDefinitionId.ToString(),
            "{}",
            nodeDtos,
            new List<WorkflowTransitionDto>()
        );

        // ACT
        await service.UpdateNodesAsync(workflowDefinitionId, "{}", input);

        // ASSERT
        workflow.Nodes.Should().HaveCount(4);
        workflow.Nodes.OfType<StartWorkflowNode>().Should().ContainSingle();
        workflow.Nodes.OfType<ApiWorkflowNode>().Should().ContainSingle();
        workflow.Nodes.OfType<NotificationWorkflowNode>().Should().ContainSingle();
        workflow.Nodes.OfType<EndWorkflowNode>().Should().ContainSingle();
    }

    [Fact]
    public async Task UpdateNodesAsync_WithDecisionNodeAndSourcePort_PreservesSourcePort()
    {
        // ARRANGE
        var mockRepository = new Mock<IWorkflowDefinitionRepository>();
        var workflowDefinitionId = WorkflowDefinitionId.Create(FixedWorkflowDefinitionId).Value;
        var publicId = WorkflowDefinitionPublicId.Create(FixedPublicId).Value;
        var workflow = WorkflowDefinition.Create(UserId.Create(FixedUserId).Value, "Test", "{}", publicId);

        mockRepository.Setup(r => r.GetByIdAsync(workflowDefinitionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflow);

        var service = new WorkflowNodeService(mockRepository.Object);

        // Generate consistent IDs for all nodes
        var startNodeId = WorkflowNodeId.New().ToString();
        var decisionNodeId = WorkflowNodeId.New().ToString();
        var endNodeId = WorkflowNodeId.New().ToString();

        var nodeDtos = new List<WorkflowNodeDto>
        {
            new WorkflowNodeDto(
                Id: startNodeId,
                NodeType: 0, // Start
                Name: "Start",
                BusinessNotes: null,
                IsConfigured: true,
                EndpointUrl: null,
                AuthToken: null,
                RetryPolicyMaxAttempts: null,
                RetryPolicyBackoffCoefficient: null,
                ContractMappingConvertXmlToJson: null,
                ContractMappingQueryParameters: null,
                ContractMappingRequestMapping: null,
                ContractMappingResponseMapping: null,
                MessageTemplate: null
            ),
            new WorkflowNodeDto(
                Id: decisionNodeId,
                NodeType: 3, // Decision
                Name: "Decision",
                BusinessNotes: null,
                IsConfigured: true,
                EndpointUrl: null,
                AuthToken: null,
                RetryPolicyMaxAttempts: null,
                RetryPolicyBackoffCoefficient: null,
                ContractMappingConvertXmlToJson: null,
                ContractMappingQueryParameters: null,
                ContractMappingRequestMapping: null,
                ContractMappingResponseMapping: null,
                MessageTemplate: null
            ),
            new WorkflowNodeDto(
                Id: endNodeId,
                NodeType: 99, // End
                Name: "End",
                BusinessNotes: null,
                IsConfigured: true,
                EndpointUrl: null,
                AuthToken: null,
                RetryPolicyMaxAttempts: null,
                RetryPolicyBackoffCoefficient: null,
                ContractMappingConvertXmlToJson: null,
                ContractMappingQueryParameters: null,
                ContractMappingRequestMapping: null,
                ContractMappingResponseMapping: null,
                MessageTemplate: null
            )
        };

        var transitionDtos = new List<WorkflowTransitionDto>
        {
            new WorkflowTransitionDto(SourceNodeId: startNodeId, TargetNodeId: decisionNodeId, SourcePort: "Default"),
            new WorkflowTransitionDto(SourceNodeId: decisionNodeId, TargetNodeId: endNodeId, SourcePort: "True"),
            new WorkflowTransitionDto(SourceNodeId: decisionNodeId, TargetNodeId: endNodeId, SourcePort: "False")
        };

        var input = new UpdateWorkflowNodesInput(
            workflowDefinitionId.ToString(),
            "{}",
            nodeDtos,
            transitionDtos
        );

        // ACT
        await service.UpdateNodesAsync(workflowDefinitionId, "{}", input);

        // ASSERT
        Console.WriteLine($"Node count: {workflow.Nodes.Count}");
        Console.WriteLine($"Transition count: {workflow.Transitions.Count}");
        Console.WriteLine($"Nodes: {string.Join(", ", workflow.Nodes.Select(n => $"{n.Type}:{n.Id}"))}");
        Console.WriteLine($"Transitions: {string.Join(", ", workflow.Transitions.Select(t => $"{t.SourceNodeId} -> {t.TargetNodeId} ({t.SourcePort})"))}");

        // Verify node IDs match transition IDs
        var startNode = workflow.Nodes.OfType<StartWorkflowNode>().First();
        var decisionNode = workflow.Nodes.OfType<DecisionWorkflowNode>().First();
        var endNode = workflow.Nodes.OfType<EndWorkflowNode>().First();

        Console.WriteLine($"Start node ID: {startNode.Id} (expected: {startNodeId})");
        Console.WriteLine($"Decision node ID: {decisionNode.Id} (expected: {decisionNodeId})");
        Console.WriteLine($"End node ID: {endNode.Id} (expected: {endNodeId})");

        workflow.Transitions.Should().HaveCount(3);
        workflow.Transitions.Count(t => t.SourcePort == "True").Should().Be(1);
        workflow.Transitions.Count(t => t.SourcePort == "False").Should().Be(1);
        workflow.Transitions.Count(t => t.SourcePort == "Default").Should().Be(1);

        var topologyResult = workflow.ValidateTopology();
        if (topologyResult.IsFailure)
        {
            Console.WriteLine($"Topology validation failed: {topologyResult.Error}");
        }
        topologyResult.IsSuccess.Should().BeTrue();
    }
    #endregion
}
