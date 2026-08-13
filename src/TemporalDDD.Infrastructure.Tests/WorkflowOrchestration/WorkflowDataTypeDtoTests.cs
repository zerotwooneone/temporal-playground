using System.Text.Json;
using TemporalDDD.Domain.WorkflowOrchestration;
using TemporalDDD.Infrastructure.WorkflowOrchestration;

namespace TemporalDDD.Infrastructure.Tests.WorkflowOrchestration;

public class WorkflowDataTypeDtoTests
{
    #region PrimitiveDto Round-Trip Tests

    [Fact]
    public void PrimitiveDto_FromDomain_WhenGivenStringPrimitive_ReturnsCorrectDto()
    {
        // ARRANGE
        var domain = WorkflowDataType.Primitive.String;

        // ACT
        var dto = WorkflowDataTypeDto.FromDomain(domain);

        // ASSERT
        dto.Should().BeOfType<WorkflowDataTypeDto.PrimitiveDto>();
        var primitiveDto = (WorkflowDataTypeDto.PrimitiveDto)dto;
        primitiveDto.Value.Should().Be(1);
        primitiveDto.Name.Should().Be("String");
        primitiveDto.TypeDiscriminator.Should().Be("Primitive");
    }

    [Fact]
    public void PrimitiveDto_FromDomain_WhenGivenNumberPrimitive_ReturnsCorrectDto()
    {
        // ARRANGE
        var domain = WorkflowDataType.Primitive.Number;

        // ACT
        var dto = WorkflowDataTypeDto.FromDomain(domain);

        // ASSERT
        dto.Should().BeOfType<WorkflowDataTypeDto.PrimitiveDto>();
        var primitiveDto = (WorkflowDataTypeDto.PrimitiveDto)dto;
        primitiveDto.Value.Should().Be(2);
        primitiveDto.Name.Should().Be("Number");
    }

    [Fact]
    public void PrimitiveDto_ToDomain_WhenGivenStringDto_ReturnsStringPrimitive()
    {
        // ARRANGE
        var dto = new WorkflowDataTypeDto.PrimitiveDto(1, "String");

        // ACT
        var domain = dto.ToDomain();

        // ASSERT
        domain.Should().Be(WorkflowDataType.Primitive.String);
    }

    [Fact]
    public void PrimitiveDto_ToDomain_WhenGivenNumberDto_ReturnsNumberPrimitive()
    {
        // ARRANGE
        var dto = new WorkflowDataTypeDto.PrimitiveDto(2, "Number");

        // ACT
        var domain = dto.ToDomain();

        // ASSERT
        domain.Should().Be(WorkflowDataType.Primitive.Number);
    }

    [Fact]
    public void PrimitiveDto_RoundTrip_WhenGivenStringPrimitive_PreservesData()
    {
        // ARRANGE
        var original = WorkflowDataType.Primitive.String;

        // ACT
        var dto = WorkflowDataTypeDto.FromDomain(original);
        var result = dto.ToDomain();

        // ASSERT
        result.Should().Be(original);
    }

    [Fact]
    public void PrimitiveDto_RoundTrip_WhenGivenBooleanPrimitive_PreservesData()
    {
        // ARRANGE
        var original = WorkflowDataType.Primitive.Boolean;

        // ACT
        var dto = WorkflowDataTypeDto.FromDomain(original);
        var result = dto.ToDomain();

        // ASSERT
        result.Should().Be(original);
    }

    [Fact]
    public void PrimitiveDto_ToDomain_WhenGivenUnknownName_ThrowsInvalidOperationException()
    {
        // ARRANGE
        var dto = new WorkflowDataTypeDto.PrimitiveDto(999, "UnknownType");

        // ACT
        var action = () => dto.ToDomain();

        // ASSERT
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Unknown primitive type*");
    }

    #endregion

    #region SemanticDto Round-Trip Tests

    [Fact]
    public void SemanticDto_FromDomain_WhenGivenSemanticType_ReturnsCorrectDto()
    {
        // ARRANGE
        var domain = new WorkflowDataType.Semantic("UserId", WorkflowDataType.Primitive.String);

        // ACT
        var dto = WorkflowDataTypeDto.FromDomain(domain);

        // ASSERT
        dto.Should().BeOfType<WorkflowDataTypeDto.SemanticDto>();
        var semanticDto = (WorkflowDataTypeDto.SemanticDto)dto;
        semanticDto.SemanticName.Should().Be("UserId");
        semanticDto.UnderlyingType.Should().BeOfType<WorkflowDataTypeDto.PrimitiveDto>();
        semanticDto.UnderlyingType.Name.Should().Be("String");
        semanticDto.TypeDiscriminator.Should().Be("Semantic");
    }

    [Fact]
    public void SemanticDto_ToDomain_WhenGivenSemanticDto_ReturnsCorrectSemanticType()
    {
        // ARRANGE
        var underlyingType = new WorkflowDataTypeDto.PrimitiveDto(1, "String");
        var dto = new WorkflowDataTypeDto.SemanticDto("UserId", underlyingType);

        // ACT
        var domain = dto.ToDomain();

        // ASSERT
        domain.Should().BeOfType<WorkflowDataType.Semantic>();
        var semantic = (WorkflowDataType.Semantic)domain;
        semantic.SemanticName.Should().Be("UserId");
        semantic.UnderlyingType.Should().Be(WorkflowDataType.Primitive.String);
    }

    [Fact]
    public void SemanticDto_RoundTrip_WhenGivenSemanticType_PreservesData()
    {
        // ARRANGE
        var original = new WorkflowDataType.Semantic("UserId", WorkflowDataType.Primitive.String);

        // ACT
        var dto = WorkflowDataTypeDto.FromDomain(original);
        var result = dto.ToDomain();

        // ASSERT
        result.Should().Be(original);
    }

    [Fact]
    public void SemanticDto_RoundTrip_WhenGivenSemanticWithNumberUnderlyingType_PreservesData()
    {
        // ARRANGE
        var original = new WorkflowDataType.Semantic("Age", WorkflowDataType.Primitive.Number);

        // ACT
        var dto = WorkflowDataTypeDto.FromDomain(original);
        var result = dto.ToDomain();

        // ASSERT
        result.Should().Be(original);
    }

    #endregion

    #region FromDomain Dispatch Tests

    [Fact]
    public void FromDomain_WhenGivenPrimitiveType_ReturnsPrimitiveDto()
    {
        // ARRANGE
        var domain = WorkflowDataType.Primitive.Json;

        // ACT
        var dto = WorkflowDataTypeDto.FromDomain(domain);

        // ASSERT
        dto.Should().BeOfType<WorkflowDataTypeDto.PrimitiveDto>();
    }

    [Fact]
    public void FromDomain_WhenGivenSemanticType_ReturnsSemanticDto()
    {
        // ARRANGE
        var domain = new WorkflowDataType.Semantic("Email", WorkflowDataType.Primitive.String);

        // ACT
        var dto = WorkflowDataTypeDto.FromDomain(domain);

        // ASSERT
        dto.Should().BeOfType<WorkflowDataTypeDto.SemanticDto>();
    }

    #endregion

    #region ToDomain Dispatch Tests

    [Fact]
    public void ToDomain_WhenGivenPrimitiveDto_ReturnsPrimitiveType()
    {
        // ARRANGE
        var dto = new WorkflowDataTypeDto.PrimitiveDto(3, "Boolean");

        // ACT
        var domain = dto.ToDomain();

        // ASSERT
        domain.Should().Be(WorkflowDataType.Primitive.Boolean);
    }

    [Fact]
    public void ToDomain_WhenGivenSemanticDto_ReturnsSemanticType()
    {
        // ARRANGE
        var underlying = new WorkflowDataTypeDto.PrimitiveDto(1, "String");
        var dto = new WorkflowDataTypeDto.SemanticDto("Email", underlying);

        // ACT
        var domain = dto.ToDomain();

        // ASSERT
        domain.Should().BeOfType<WorkflowDataType.Semantic>();
    }

    #endregion
}
