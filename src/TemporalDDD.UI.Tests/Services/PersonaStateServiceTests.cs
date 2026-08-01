using TemporalDDD.UI.Services;

namespace TemporalDDD.UI.Tests.Services;

public class PersonaStateServiceTests
{
    #region Constructor Tests
    [Fact]
    public void Constructor_WhenInitialized_SetsDefaultRoleToAdmin()
    {
        // Arrange & Act
        var service = new PersonaStateService();

        // Assert
        service.CurrentUserRole.Should().Be(UserRole.Admin);
    }
    #endregion

    #region CurrentUserRole Tests
    [Fact]
    public void CurrentUserRole_WhenSetToNewValue_RaisesOnChange()
    {
        // Arrange
        var service = new PersonaStateService();
        var changeRaised = false;
        service.OnChange += () => changeRaised = true;

        // Act
        service.CurrentUserRole = UserRole.PlacementSpecialist;

        // Assert
        changeRaised.Should().BeTrue();
        service.CurrentUserRole.Should().Be(UserRole.PlacementSpecialist);
    }

    [Fact]
    public void CurrentUserRole_WhenSetToSameValue_DoesNotRaiseOnChange()
    {
        // Arrange
        var service = new PersonaStateService();
        var changeRaised = false;
        service.OnChange += () => changeRaised = true;

        // Act
        service.CurrentUserRole = UserRole.Admin; // Same as initial value

        // Assert
        changeRaised.Should().BeFalse();
    }
    #endregion

    #region GetRoleDisplayName Tests
    [Fact]
    public void GetRoleDisplayName_WhenRoleIsAdmin_ReturnsAdmin()
    {
        // Arrange
        var service = new PersonaStateService();

        // Act
        var displayName = service.GetRoleDisplayName(UserRole.Admin);

        // Assert
        displayName.Should().Be("Admin");
    }

    [Fact]
    public void GetRoleDisplayName_WhenRoleIsPlacementSpecialist_ReturnsMarcus()
    {
        // Arrange
        var service = new PersonaStateService();

        // Act
        var displayName = service.GetRoleDisplayName(UserRole.PlacementSpecialist);

        // Assert
        displayName.Should().Be("Marcus (Placement Specialist)");
    }

    [Fact]
    public void GetRoleDisplayName_WhenRoleIsCredentialing_ReturnsSarah()
    {
        // Arrange
        var service = new PersonaStateService();

        // Act
        var displayName = service.GetRoleDisplayName(UserRole.Credentialing);

        // Assert
        displayName.Should().Be("Sarah (Credentialing)");
    }

    [Fact]
    public void GetRoleDisplayName_WhenRoleIsProvider_ReturnsDrEmily()
    {
        // Arrange
        var service = new PersonaStateService();

        // Act
        var displayName = service.GetRoleDisplayName(UserRole.Provider);

        // Assert
        displayName.Should().Be("Dr. Emily (Provider)");
    }

    [Fact]
    public void GetRoleDisplayName_WhenRoleIsUnknown_ReturnsUnknown()
    {
        // Arrange
        var service = new PersonaStateService();

        // Act
        var displayName = service.GetRoleDisplayName((UserRole)999);

        // Assert
        displayName.Should().Be("Unknown");
    }
    #endregion

    #region OnChange Subscription Tests
    [Fact]
    public void OnChange_WhenSubscribedAndUnsubscribed_OnlyRaisesWhenSubscribed()
    {
        // Arrange
        var service = new PersonaStateService();
        var changeCount = 0;
        Action handler = () => changeCount++;
        
        // Subscribe
        service.OnChange += handler;
        service.CurrentUserRole = UserRole.PlacementSpecialist;
        var firstChangeCount = changeCount;
        
        // Unsubscribe
        service.OnChange -= handler;
        service.CurrentUserRole = UserRole.Credentialing;
        
        // Assert
        firstChangeCount.Should().Be(1);
        changeCount.Should().Be(1); // Should not have increased after unsubscribe
    }
    #endregion
}
