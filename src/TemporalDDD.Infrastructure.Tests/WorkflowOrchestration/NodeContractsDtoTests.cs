using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Infrastructure.WorkflowOrchestration;

namespace TemporalDDD.Infrastructure.Tests.WorkflowOrchestration;

public class NodeContractsDtoTests
{
    #region NodeInputDefinitionDto Tests

    [Fact]
    public void NodeInputDefinitionDto_ToDto_WhenGivenInputDefinition_ReturnsCorrectDto()
    {
        // ARRANGE
        var domain = new NodeInputDefinition("EndpointUrl", WorkflowDataType.Primitive.String, true);

        // ACT
        var dto = NodeContractsDtoMapper.ToDto(domain);

        // ASSERT
        dto.PropertyName.Should().Be("EndpointUrl");
        dto.DataType.Should().BeOfType<WorkflowDataTypeDto.PrimitiveDto>();
        dto.IsRequired.Should().BeTrue();
    }

    [Fact]
    public void NodeInputDefinitionDto_ToDomain_WhenGivenDto_ReturnsCorrectInputDefinition()
    {
        // ARRANGE
        var dataTypeDto = new WorkflowDataTypeDto.PrimitiveDto(1, "String");
        var dto = new NodeInputDefinitionDto("EndpointUrl", dataTypeDto, true);

        // ACT
        var domain = NodeContractsDtoMapper.ToDomain(dto);

        // ASSERT
        domain.PropertyName.Should().Be("EndpointUrl");
        domain.DataType.Should().Be(WorkflowDataType.Primitive.String);
        domain.IsRequired.Should().BeTrue();
    }

    [Fact]
    public void NodeInputDefinitionDto_RoundTrip_WhenGivenInputDefinition_PreservesData()
    {
        // ARRANGE
        var original = new NodeInputDefinition("AuthToken", WorkflowDataType.Primitive.String, false);

        // ACT
        var dto = NodeContractsDtoMapper.ToDto(original);
        var result = NodeContractsDtoMapper.ToDomain(dto);

        // ASSERT
        result.PropertyName.Should().Be(original.PropertyName);
        result.DataType.Should().Be(original.DataType);
        result.IsRequired.Should().Be(original.IsRequired);
    }

    [Fact]
    public void NodeInputDefinitionDto_RoundTrip_WhenGivenInputWithSemanticType_PreservesData()
    {
        // ARRANGE
        var semanticType = new WorkflowDataType.Semantic("UserId", WorkflowDataType.Primitive.String);
        var original = new NodeInputDefinition("UserId", semanticType, true);

        // ACT
        var dto = NodeContractsDtoMapper.ToDto(original);
        var result = NodeContractsDtoMapper.ToDomain(dto);

        // ASSERT
        result.PropertyName.Should().Be(original.PropertyName);
        result.DataType.Should().Be(original.DataType);
        result.IsRequired.Should().Be(original.IsRequired);
    }

    #endregion

    #region NodeOutputDefinitionDto Tests

    [Fact]
    public void NodeOutputDefinitionDto_ToDto_WhenGivenOutputDefinition_ReturnsCorrectDto()
    {
        // ARRANGE
        var domain = new NodeOutputDefinition("ApiResponse", WorkflowDataType.Primitive.Json);

        // ACT
        var dto = NodeContractsDtoMapper.ToDto(domain);

        // ASSERT
        dto.PropertyName.Should().Be("ApiResponse");
        dto.DataType.Should().BeOfType<WorkflowDataTypeDto.PrimitiveDto>();
    }

    [Fact]
    public void NodeOutputDefinitionDto_ToDomain_WhenGivenDto_ReturnsCorrectOutputDefinition()
    {
        // ARRANGE
        var dataTypeDto = new WorkflowDataTypeDto.PrimitiveDto(5, "JsonDocument");
        var dto = new NodeOutputDefinitionDto("ApiResponse", dataTypeDto);

        // ACT
        var domain = NodeContractsDtoMapper.ToDomain(dto);

        // ASSERT
        domain.PropertyName.Should().Be("ApiResponse");
        domain.DataType.Should().Be(WorkflowDataType.Primitive.Json);
    }

    [Fact]
    public void NodeOutputDefinitionDto_RoundTrip_WhenGivenOutputDefinition_PreservesData()
    {
        // ARRANGE
        var original = new NodeOutputDefinition("Result", WorkflowDataType.Primitive.Boolean);

        // ACT
        var dto = NodeContractsDtoMapper.ToDto(original);
        var result = NodeContractsDtoMapper.ToDomain(dto);

        // ASSERT
        result.PropertyName.Should().Be(original.PropertyName);
        result.DataType.Should().Be(original.DataType);
    }

    [Fact]
    public void NodeOutputDefinitionDto_RoundTrip_WhenGivenOutputWithSemanticType_PreservesData()
    {
        // ARRANGE
        var semanticType = new WorkflowDataType.Semantic("UserEmail", WorkflowDataType.Primitive.String);
        var original = new NodeOutputDefinition("Email", semanticType);

        // ACT
        var dto = NodeContractsDtoMapper.ToDto(original);
        var result = NodeContractsDtoMapper.ToDomain(dto);

        // ASSERT
        result.PropertyName.Should().Be(original.PropertyName);
        result.DataType.Should().Be(original.DataType);
    }

    #endregion

    #region List Conversion Tests

    [Fact]
    public void ToDto_WhenGivenListOfInputDefinitions_ReturnsListOfDtos()
    {
        // ARRANGE
        var inputs = new List<NodeInputDefinition>
        {
            new("EndpointUrl", WorkflowDataType.Primitive.String, true),
            new("AuthToken", WorkflowDataType.Primitive.String, false)
        };

        // ACT
        var dtos = inputs.Select(NodeContractsDtoMapper.ToDto).ToList();

        // ASSERT
        dtos.Should().HaveCount(2);
        dtos[0].PropertyName.Should().Be("EndpointUrl");
        dtos[1].PropertyName.Should().Be("AuthToken");
    }

    [Fact]
    public void ToDomain_WhenGivenListOfInputDtos_ReturnsListOfInputDefinitions()
    {
        // ARRANGE
        var dtos = new List<NodeInputDefinitionDto>
        {
            new("EndpointUrl", new WorkflowDataTypeDto.PrimitiveDto(1, "String"), true),
            new("AuthToken", new WorkflowDataTypeDto.PrimitiveDto(1, "String"), false)
        };

        // ACT
        var domains = dtos.Select(NodeContractsDtoMapper.ToDomain).ToList();

        // ASSERT
        domains.Should().HaveCount(2);
        domains[0].PropertyName.Should().Be("EndpointUrl");
        domains[1].PropertyName.Should().Be("AuthToken");
    }

    [Fact]
    public void ToDto_WhenGivenListOfOutputDefinitions_ReturnsListOfDtos()
    {
        // ARRANGE
        var outputs = new List<NodeOutputDefinition>
        {
            new("ApiResponse", WorkflowDataType.Primitive.Json),
            new("StatusCode", WorkflowDataType.Primitive.Number)
        };

        // ACT
        var dtos = outputs.Select(NodeContractsDtoMapper.ToDto).ToList();

        // ASSERT
        dtos.Should().HaveCount(2);
        dtos[0].PropertyName.Should().Be("ApiResponse");
        dtos[1].PropertyName.Should().Be("StatusCode");
    }

    [Fact]
    public void ToDomain_WhenGivenListOfOutputDtos_ReturnsListOfOutputDefinitions()
    {
        // ARRANGE
        var dtos = new List<NodeOutputDefinitionDto>
        {
            new("ApiResponse", new WorkflowDataTypeDto.PrimitiveDto(5, "JsonDocument")),
            new("StatusCode", new WorkflowDataTypeDto.PrimitiveDto(2, "Number"))
        };

        // ACT
        var domains = dtos.Select(NodeContractsDtoMapper.ToDomain).ToList();

        // ASSERT
        domains.Should().HaveCount(2);
        domains[0].PropertyName.Should().Be("ApiResponse");
        domains[1].PropertyName.Should().Be("StatusCode");
    }

    #endregion
}
