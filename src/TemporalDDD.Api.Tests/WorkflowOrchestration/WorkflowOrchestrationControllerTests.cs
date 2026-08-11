using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using TemporalDDD.Api.WorkflowOrchestration;
using TemporalDDD.Application.WorkflowOrchestration;
using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Domain.WorkflowOrchestration.ValueObjects;
using ApiWorkflowNodeDto = TemporalDDD.Api.WorkflowOrchestration.WorkflowNodeDto;
using ApiWorkflowTransitionDto = TemporalDDD.Api.WorkflowOrchestration.WorkflowTransitionDto;

namespace TemporalDDD.Api.Tests.WorkflowOrchestration;

public class WorkflowOrchestrationControllerTests
{
    private readonly Mock<IWorkflowDefinitionQuery> _mockQuery;
    private readonly Mock<IWorkflowCodeGeneratorService> _mockCodeGeneratorService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IWorkflowDefinitionRepository> _mockRepository;
    private readonly Mock<IWorkflowNodeService> _mockWorkflowNodeService;

    public WorkflowOrchestrationControllerTests()
    {
        _mockQuery = new Mock<IWorkflowDefinitionQuery>();
        _mockCodeGeneratorService = new Mock<IWorkflowCodeGeneratorService>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockRepository = new Mock<IWorkflowDefinitionRepository>();
        _mockWorkflowNodeService = new Mock<IWorkflowNodeService>();
    }

    #region CreateWorkflowDraft Tests
    [Fact]
    public async Task CreateWorkflowDraft_WhenCreatorIdIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateWorkflowRequest(
            CreatorId: "INVALID_ID",
            Name: "Test Workflow"
        );

        var controller = new WorkflowOrchestrationController(
            _mockQuery.Object,
            _mockCodeGeneratorService.Object,
            _mockConfiguration.Object,
            _mockRepository.Object,
            _mockWorkflowNodeService.Object
        );

        // Act
        var result = await controller.CreateWorkflowDraft(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().BeOfType<string>();
    }

    [Fact]
    public async Task CreateWorkflowDraft_WhenNameIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateWorkflowRequest(
            CreatorId: "USRId00000000-0000-0000-0000-000000000001",
            Name: ""
        );

        var controller = new WorkflowOrchestrationController(
            _mockQuery.Object,
            _mockCodeGeneratorService.Object,
            _mockConfiguration.Object,
            _mockRepository.Object,
            _mockWorkflowNodeService.Object
        );

        // Act
        var result = await controller.CreateWorkflowDraft(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().Be("Name is required and cannot be empty.");
    }

    [Fact]
    public async Task CreateWorkflowDraft_WhenNameIsWhitespace_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateWorkflowRequest(
            CreatorId: "USRId00000000-0000-0000-0000-000000000001",
            Name: "   "
        );

        var controller = new WorkflowOrchestrationController(
            _mockQuery.Object,
            _mockCodeGeneratorService.Object,
            _mockConfiguration.Object,
            _mockRepository.Object,
            _mockWorkflowNodeService.Object
        );

        // Act
        var result = await controller.CreateWorkflowDraft(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().Be("Name is required and cannot be empty.");
    }
    #endregion

    #region UpdateWorkflowNodes Tests
    [Fact]
    public async Task UpdateWorkflowNodes_WhenPublicIdIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var request = new UpdateWorkflowNodesRequest(
            FlowJson: "{}",
            Nodes: new List<ApiWorkflowNodeDto>(),
            Transitions: new List<ApiWorkflowTransitionDto>()
        );

        var controller = new WorkflowOrchestrationController(
            _mockQuery.Object,
            _mockCodeGeneratorService.Object,
            _mockConfiguration.Object,
            _mockRepository.Object,
            _mockWorkflowNodeService.Object
        );

        // Act
        var result = await controller.UpdateWorkflowNodes("INVALID_ID", request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().BeOfType<string>();
    }

    [Fact]
    public async Task UpdateWorkflowNodes_WhenFlowJsonIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var publicId = "WFL_00000000-0000-0000-0000-000000000001";
        var workflowDefinitionId = WorkflowDefinitionId.New();
        _mockQuery
            .Setup(x => x.GetWorkflowDefinitionIdByPublicIdAsync(It.IsAny<WorkflowDefinitionPublicId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflowDefinitionId);

        var request = new UpdateWorkflowNodesRequest(
            FlowJson: "",
            Nodes: new List<ApiWorkflowNodeDto>(),
            Transitions: new List<ApiWorkflowTransitionDto>()
        );

        var controller = new WorkflowOrchestrationController(
            _mockQuery.Object,
            _mockCodeGeneratorService.Object,
            _mockConfiguration.Object,
            _mockRepository.Object,
            _mockWorkflowNodeService.Object
        );

        // Act
        var result = await controller.UpdateWorkflowNodes(publicId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().Be("FlowJson is required");
    }

    [Fact]
    public async Task UpdateWorkflowNodes_WhenFlowJsonExceedsMaxLength_ReturnsBadRequest()
    {
        // Arrange
        var publicId = "WFL_00000000-0000-0000-0000-000000000001";
        var workflowDefinitionId = WorkflowDefinitionId.New();
        _mockQuery
            .Setup(x => x.GetWorkflowDefinitionIdByPublicIdAsync(It.IsAny<WorkflowDefinitionPublicId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflowDefinitionId);

        var largeJson = new string('a', 1_048_577); // 1MB + 1 character
        var request = new UpdateWorkflowNodesRequest(
            FlowJson: largeJson,
            Nodes: new List<ApiWorkflowNodeDto>(),
            Transitions: new List<ApiWorkflowTransitionDto>()
        );

        var controller = new WorkflowOrchestrationController(
            _mockQuery.Object,
            _mockCodeGeneratorService.Object,
            _mockConfiguration.Object,
            _mockRepository.Object,
            _mockWorkflowNodeService.Object
        );

        // Act
        var result = await controller.UpdateWorkflowNodes(publicId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().Be("FlowJson exceeds maximum length of 1048576 characters");
    }

    [Fact]
    public async Task UpdateWorkflowNodes_WhenFlowJsonIsInvalidJson_ReturnsBadRequest()
    {
        // Arrange
        var publicId = "WFL_00000000-0000-0000-0000-000000000001";
        var workflowDefinitionId = WorkflowDefinitionId.New();
        _mockQuery
            .Setup(x => x.GetWorkflowDefinitionIdByPublicIdAsync(It.IsAny<WorkflowDefinitionPublicId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflowDefinitionId);

        var request = new UpdateWorkflowNodesRequest(
            FlowJson: "not valid json",
            Nodes: new List<ApiWorkflowNodeDto>(),
            Transitions: new List<ApiWorkflowTransitionDto>()
        );

        var controller = new WorkflowOrchestrationController(
            _mockQuery.Object,
            _mockCodeGeneratorService.Object,
            _mockConfiguration.Object,
            _mockRepository.Object,
            _mockWorkflowNodeService.Object
        );

        // Act
        var result = await controller.UpdateWorkflowNodes(publicId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().Be("FlowJson is not valid JSON");
    }

    [Fact]
    public async Task UpdateWorkflowNodes_WhenFlowJsonMissingNodes_ReturnsBadRequest()
    {
        // Arrange
        var publicId = "WFL_00000000-0000-0000-0000-000000000001";
        var workflowDefinitionId = WorkflowDefinitionId.New();
        _mockQuery
            .Setup(x => x.GetWorkflowDefinitionIdByPublicIdAsync(It.IsAny<WorkflowDefinitionPublicId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflowDefinitionId);

        var request = new UpdateWorkflowNodesRequest(
            FlowJson: "{\"edges\":[]}",
            Nodes: new List<ApiWorkflowNodeDto>(),
            Transitions: new List<ApiWorkflowTransitionDto>()
        );

        var controller = new WorkflowOrchestrationController(
            _mockQuery.Object,
            _mockCodeGeneratorService.Object,
            _mockConfiguration.Object,
            _mockRepository.Object,
            _mockWorkflowNodeService.Object
        );

        // Act
        var result = await controller.UpdateWorkflowNodes(publicId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().Be("FlowJson must contain a 'nodes' array");
    }

    [Fact]
    public async Task UpdateWorkflowNodes_WhenFlowJsonMissingEdges_ReturnsBadRequest()
    {
        // Arrange
        var publicId = "WFL_00000000-0000-0000-0000-000000000001";
        var workflowDefinitionId = WorkflowDefinitionId.New();
        _mockQuery
            .Setup(x => x.GetWorkflowDefinitionIdByPublicIdAsync(It.IsAny<WorkflowDefinitionPublicId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflowDefinitionId);

        var request = new UpdateWorkflowNodesRequest(
            FlowJson: "{\"nodes\":[]}",
            Nodes: new List<ApiWorkflowNodeDto>(),
            Transitions: new List<ApiWorkflowTransitionDto>()
        );

        var controller = new WorkflowOrchestrationController(
            _mockQuery.Object,
            _mockCodeGeneratorService.Object,
            _mockConfiguration.Object,
            _mockRepository.Object,
            _mockWorkflowNodeService.Object
        );

        // Act
        var result = await controller.UpdateWorkflowNodes(publicId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().Be("FlowJson must contain an 'edges' array");
    }

    [Fact]
    public async Task UpdateWorkflowNodes_WhenNodeIdIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var publicId = "WFL_00000000-0000-0000-0000-000000000001";
        var workflowDefinitionId = WorkflowDefinitionId.New();
        _mockQuery
            .Setup(x => x.GetWorkflowDefinitionIdByPublicIdAsync(It.IsAny<WorkflowDefinitionPublicId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflowDefinitionId);

        var request = new UpdateWorkflowNodesRequest(
            FlowJson: "{\"nodes\":[],\"edges\":[]}",
            Nodes: new List<ApiWorkflowNodeDto>
            {
                new ApiWorkflowNodeDto(
                    Id: "INVALID_ID",
                    NodeType: 0,
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
                )
            },
            Transitions: new List<ApiWorkflowTransitionDto>()
        );

        var controller = new WorkflowOrchestrationController(
            _mockQuery.Object,
            _mockCodeGeneratorService.Object,
            _mockConfiguration.Object,
            _mockRepository.Object,
            _mockWorkflowNodeService.Object
        );

        // Act
        var result = await controller.UpdateWorkflowNodes(publicId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().BeOfType<string>();
    }

    [Fact]
    public async Task UpdateWorkflowNodes_WhenNodeTypeIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var publicId = "WFL_00000000-0000-0000-0000-000000000001";
        var workflowDefinitionId = WorkflowDefinitionId.New();
        _mockQuery
            .Setup(x => x.GetWorkflowDefinitionIdByPublicIdAsync(It.IsAny<WorkflowDefinitionPublicId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflowDefinitionId);

        var request = new UpdateWorkflowNodesRequest(
            FlowJson: "{\"nodes\":[],\"edges\":[]}",
            Nodes: new List<ApiWorkflowNodeDto>
            {
                new ApiWorkflowNodeDto(
                    Id: "WFNId00000000-0000-0000-0000-000000000001",
                    NodeType: 999, // Invalid node type
                    Name: "Invalid",
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
            },
            Transitions: new List<ApiWorkflowTransitionDto>()
        );

        var controller = new WorkflowOrchestrationController(
            _mockQuery.Object,
            _mockCodeGeneratorService.Object,
            _mockConfiguration.Object,
            _mockRepository.Object,
            _mockWorkflowNodeService.Object
        );

        // Act
        var result = await controller.UpdateWorkflowNodes(publicId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().BeOfType<string>();
    }
    #endregion

    #region PublishWorkflow Tests
    [Fact]
    public async Task PublishWorkflow_WhenWorkflowNotApproved_ReturnsBadRequest()
    {
        // Arrange
        var publicId = "WFL_00000000-0000-0000-0000-000000000001";
        var workflowDefinitionId = WorkflowDefinitionId.New();
        var workflowDefinition = WorkflowDefinition.Create(
            UserId.New(),
            "Test Workflow",
            "{}",
            WorkflowDefinitionPublicId.Create(publicId).Value
        );

        _mockQuery
            .Setup(x => x.GetWorkflowDefinitionIdByPublicIdAsync(It.IsAny<WorkflowDefinitionPublicId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflowDefinitionId);
        _mockRepository
            .Setup(x => x.GetByIdAsync(workflowDefinitionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(workflowDefinition);
        _mockConfiguration
            .Setup(x => x["WorkerProjectPath"])
            .Returns("C:\\TestPath");

        var controller = new WorkflowOrchestrationController(
            _mockQuery.Object,
            _mockCodeGeneratorService.Object,
            _mockConfiguration.Object,
            _mockRepository.Object,
            _mockWorkflowNodeService.Object
        );

        // Act
        var result = await controller.PublishWorkflow(publicId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().Be($"Workflow must be in Approved status before publishing. Current status: {WorkflowStatus.Draft}");
    }
    #endregion

    #region GetWorkflowById Tests
    [Fact]
    public async Task GetWorkflowById_WhenWorkflowNotFound_ReturnsNotFound()
    {
        // Arrange
        var publicId = "WFL_00000000-0000-0000-0000-000000000001";
        _mockQuery
            .Setup(x => x.GetWorkflowByPublicIdAsync(It.IsAny<WorkflowDefinitionPublicId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((WorkflowDetailDto?)null);

        var controller = new WorkflowOrchestrationController(
            _mockQuery.Object,
            _mockCodeGeneratorService.Object,
            _mockConfiguration.Object,
            _mockRepository.Object,
            _mockWorkflowNodeService.Object
        );

        // Act
        var result = await controller.GetWorkflowById(publicId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult.Should().NotBeNull();
        notFoundResult!.Value.Should().Be($"Workflow with ID '{publicId}' not found");
    }
    #endregion
}
