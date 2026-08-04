using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.WorkflowOrchestration;

namespace TemporalDDD.Domain.Tests.IdentityAndAccess;

public class StronglyTypedIdTests
{
    #region UserId Tests
    [Fact]
    public void UserId_Create_WithValidPrefixAndGuid_ReturnsSuccess()
    {
        // ARRANGE
        var guid = Guid.NewGuid();
        var validId = $"USRId{guid}";

        // ACT
        var result = UserId.Create(validId);

        // ASSERT
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(guid);
    }

    [Fact]
    public void UserId_Create_WithInvalidPrefix_ReturnsFailure()
    {
        // ARRANGE
        var guid = Guid.NewGuid();
        var invalidId = $"INVALID{guid}";

        // ACT
        var result = UserId.Create(invalidId);

        // ASSERT
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("must start with 'USRId'");
    }

    [Fact]
    public void UserId_Create_WithInvalidGuid_ReturnsFailure()
    {
        // ARRANGE
        var invalidId = "USRIdNotAGuid";

        // ACT
        var result = UserId.Create(invalidId);

        // ASSERT
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid GUID format");
    }

    [Fact]
    public void UserId_Create_WithNullOrEmpty_ReturnsFailure()
    {
        // ACT
        var result1 = UserId.Create(null!);
        var result2 = UserId.Create("");

        // ASSERT
        result1.IsFailure.Should().BeTrue();
        result2.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void UserId_New_GeneratesValidIdWithCorrectFormat()
    {
        // ACT
        var userId = UserId.New();

        // ASSERT
        userId.Value.Should().NotBe(Guid.Empty);
        userId.ToString().Should().StartWith("USRId");
    }

    [Fact]
    public void UserId_ToString_ReturnsCorrectFormat()
    {
        // ARRANGE
        var userId = UserId.New();
        var guid = userId.Value;

        // ACT
        var result = userId.ToString();

        // ASSERT
        result.Should().Be($"USRId{guid}");
    }
    #endregion

    #region UserPublicId Tests
    [Fact]
    public void UserPublicId_Create_WithValidPrefixAndGuid_ReturnsSuccess()
    {
        // ARRANGE
        var guid = Guid.NewGuid();
        var validId = $"USR_{guid:N}";

        // ACT
        var result = UserPublicId.Create(validId);

        // ASSERT
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(guid);
    }

    [Fact]
    public void UserPublicId_Create_WithInvalidPrefix_ReturnsFailure()
    {
        // ARRANGE
        var guid = Guid.NewGuid();
        var invalidId = $"INVALID_{guid:N}";

        // ACT
        var result = UserPublicId.Create(invalidId);

        // ASSERT
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("must have prefix 'USR'");
    }

    [Fact]
    public void UserPublicId_Create_WithInvalidGuid_ReturnsFailure()
    {
        // ARRANGE
        var invalidId = "USR_NotAGuid";

        // ACT
        var result = UserPublicId.Create(invalidId);

        // ASSERT
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid GUID format");
    }

    [Fact]
    public void UserPublicId_New_GeneratesValidId()
    {
        // ACT
        var publicId = UserPublicId.New();

        // ASSERT
        publicId.Value.Should().NotBe(Guid.Empty);
        publicId.ToString().Should().StartWith("USR_");
    }

    [Fact]
    public void UserPublicId_ToString_ReturnsCorrectFormat()
    {
        // ARRANGE
        var publicId = UserPublicId.New();
        var guid = publicId.Value;

        // ACT
        var result = publicId.ToString();

        // ASSERT
        result.Should().Be($"USR_{guid:N}");
    }
    #endregion

    #region RoleId Tests
    [Fact]
    public void RoleId_Create_WithValidPrefixAndGuid_ReturnsSuccess()
    {
        // ARRANGE
        var guid = Guid.NewGuid();
        var validId = $"ROLId{guid}";

        // ACT
        var result = RoleId.Create(validId);

        // ASSERT
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(guid);
    }

    [Fact]
    public void RoleId_Create_WithInvalidPrefix_ReturnsFailure()
    {
        // ARRANGE
        var guid = Guid.NewGuid();
        var invalidId = $"INVALID{guid}";

        // ACT
        var result = RoleId.Create(invalidId);

        // ASSERT
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("must start with 'ROLId'");
    }

    [Fact]
    public void RoleId_New_GeneratesValidIdWithCorrectFormat()
    {
        // ACT
        var roleId = RoleId.New();

        // ASSERT
        roleId.Value.Should().NotBe(Guid.Empty);
        roleId.ToString().Should().StartWith("ROLId");
    }
    #endregion

    #region WorkflowDefinitionId Tests
    [Fact]
    public void WorkflowDefinitionId_Create_WithValidPrefixAndGuid_ReturnsSuccess()
    {
        // ARRANGE
        var guid = Guid.NewGuid();
        var validId = $"WFLId{guid}";

        // ACT
        var result = WorkflowDefinitionId.Create(validId);

        // ASSERT
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(guid);
    }

    [Fact]
    public void WorkflowDefinitionId_Create_WithInvalidPrefix_ReturnsFailure()
    {
        // ARRANGE
        var guid = Guid.NewGuid();
        var invalidId = $"INVALID{guid}";

        // ACT
        var result = WorkflowDefinitionId.Create(invalidId);

        // ASSERT
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("must start with 'WFLId'");
    }

    [Fact]
    public void WorkflowDefinitionId_New_GeneratesValidIdWithCorrectFormat()
    {
        // ACT
        var workflowId = WorkflowDefinitionId.New();

        // ASSERT
        workflowId.Value.Should().NotBe(Guid.Empty);
        workflowId.ToString().Should().StartWith("WFLId");
    }
    #endregion

    #region WorkflowDefinitionPublicId Tests
    [Fact]
    public void WorkflowDefinitionPublicId_Create_WithValidPrefixAndGuid_ReturnsSuccess()
    {
        // ARRANGE
        var guid = Guid.NewGuid();
        var validId = $"WFL_{guid:N}";

        // ACT
        var result = WorkflowDefinitionPublicId.Create(validId);

        // ASSERT
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(guid);
    }

    [Fact]
    public void WorkflowDefinitionPublicId_Create_WithInvalidPrefix_ReturnsFailure()
    {
        // ARRANGE
        var guid = Guid.NewGuid();
        var invalidId = $"INVALID_{guid:N}";

        // ACT
        var result = WorkflowDefinitionPublicId.Create(invalidId);

        // ASSERT
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("must have prefix 'WFL'");
    }

    [Fact]
    public void WorkflowDefinitionPublicId_New_GeneratesValidId()
    {
        // ACT
        var publicId = WorkflowDefinitionPublicId.New();

        // ASSERT
        publicId.Value.Should().NotBe(Guid.Empty);
        publicId.ToString().Should().StartWith("WFL_");
    }
    #endregion

    #region WorkflowNodeId Tests
    [Fact]
    public void WorkflowNodeId_Create_WithValidPrefixAndGuid_ReturnsSuccess()
    {
        // ARRANGE
        var guid = Guid.NewGuid();
        var validId = $"WFNId{guid}";

        // ACT
        var result = WorkflowNodeId.Create(validId);

        // ASSERT
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(guid);
    }

    [Fact]
    public void WorkflowNodeId_Create_WithInvalidPrefix_ReturnsFailure()
    {
        // ARRANGE
        var guid = Guid.NewGuid();
        var invalidId = $"INVALID{guid}";

        // ACT
        var result = WorkflowNodeId.Create(invalidId);

        // ASSERT
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("must start with 'WFNId'");
    }

    [Fact]
    public void WorkflowNodeId_New_GeneratesValidIdWithCorrectFormat()
    {
        // ACT
        var nodeId = WorkflowNodeId.New();

        // ASSERT
        nodeId.Value.Should().NotBe(Guid.Empty);
        nodeId.ToString().Should().StartWith("WFNId");
    }
    #endregion
}
