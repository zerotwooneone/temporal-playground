using Microsoft.AspNetCore.Mvc;
using Moq;
using Temporalio.Client;
using TemporalDDD.Api.ProviderOnboarding;

namespace TemporalDDD.Api.Tests.ProviderOnboarding;

public class OnboardingControllerTests
{
    private readonly Mock<ITemporalClient> _mockTemporalClient;

    public OnboardingControllerTests()
    {
        _mockTemporalClient = new Mock<ITemporalClient>();
    }

    #region StartOnboarding Tests
    [Fact]
    public async Task StartOnboarding_WhenProviderIdIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var request = new OnboardingController.OnboardingRequest(
            ProviderId: "INVALID_ID",
            LicenseNumber: "LICENSE123456"
        );

        var controller = new OnboardingController(_mockTemporalClient.Object);

        // Act
        var result = await controller.StartOnboarding(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().BeOfType<string>();
    }

    [Fact]
    public async Task StartOnboarding_WhenLicenseNumberIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var request = new OnboardingController.OnboardingRequest(
            ProviderId: "PROVId00000000-0000-0000-0000-000000000001",
            LicenseNumber: "INVALID@#" // Contains invalid characters
        );

        var controller = new OnboardingController(_mockTemporalClient.Object);

        // Act
        var result = await controller.StartOnboarding(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().BeOfType<string>();
    }
    #endregion
}
