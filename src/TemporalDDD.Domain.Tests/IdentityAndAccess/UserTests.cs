using TemporalDDD.Domain.IdentityAndAccess;
using TemporalDDD.Domain.IdentityAndAccess.Events;

namespace TemporalDDD.Domain.Tests.IdentityAndAccess;

public class UserTests
{
    #region Create Tests
    [Fact]
    public void Create_WithValidParameters_GeneratesIdsAndRaisesEvent()
    {
        // ARRANGE
        var username = "testuser";
        var email = "test@example.com";

        // ACT
        var user = User.Create(username, email);

        // ASSERT
        user.Username.Should().Be(username);
        user.Email.Should().Be(email);
        user.IsActive.Should().BeTrue();
        user.Id.Should().NotBeNull();
        user.PublicId.Should().NotBeNull();
        user.DomainEvents.Should().ContainSingle(e => e is UserCreated);
    }
    #endregion

    #region AssignRole Tests
    [Fact]
    public void AssignRole_WithNewRole_AddsRoleAndRaisesEvent()
    {
        // ARRANGE
        var user = User.Create("testuser", "test@example.com");
        var roleId = RoleId.New();

        // ACT
        user.AssignRole(roleId);

        // ASSERT
        user.AssignedRoles.Should().Contain(roleId);
        user.DomainEvents.Should().ContainSingle(e => e is RoleAssignedToUser);
    }

    [Fact]
    public void AssignRole_WithDuplicateRole_DoesNotAddDuplicate()
    {
        // ARRANGE
        var user = User.Create("testuser", "test@example.com");
        var roleId = RoleId.New();
        user.AssignRole(roleId);

        // ACT
        user.AssignRole(roleId);

        // ASSERT
        user.AssignedRoles.Count.Should().Be(1);
        user.AssignedRoles.Should().Contain(roleId);
        user.DomainEvents.OfType<RoleAssignedToUser>().Count().Should().Be(1);
    }

    [Fact]
    public void AssignedRoles_IsReadOnlyCollection()
    {
        // ARRANGE
        var user = User.Create("testuser", "test@example.com");

        // ACT
        var roles = user.AssignedRoles;

        // ASSERT
        roles.Should().BeAssignableTo<System.Collections.Generic.IReadOnlyCollection<RoleId>>();
    }
    #endregion
}
