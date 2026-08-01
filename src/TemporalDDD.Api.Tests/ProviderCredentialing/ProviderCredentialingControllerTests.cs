using Microsoft.AspNetCore.Mvc;
using Moq;
using Temporalio.Client;
using TemporalDDD.Api.ProviderCredentialing;
using TemporalDDD.Application.ProviderCredentialing;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.Testing;

namespace TemporalDDD.Api.Tests.ProviderCredentialing;

public class ProviderCredentialingControllerTests
{
    private static readonly DateOnly FixedCurrentDate = new DateOnly(2026, 1, 1);
    private static readonly DateOnly FixedFutureDate = new DateOnly(2028, 1, 1);
    private static readonly DateOnly FixedPastDate = new DateOnly(2025, 1, 1);

    private readonly Mock<ITemporalClient> _mockTemporalClient;
    private readonly Mock<IPendingManualReviewsQuery> _mockPendingReviewsQuery;
    private readonly ITimeProvider _timeProvider;

    public ProviderCredentialingControllerTests()
    {
        _mockTemporalClient = new Mock<ITemporalClient>();
        _mockPendingReviewsQuery = new Mock<IPendingManualReviewsQuery>();
        _timeProvider = new FixedTimeProvider(new DateTimeOffset(FixedCurrentDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero));
    }

    #region StartCredentialing Tests
    [Fact]
    public async Task StartCredentialing_WhenLicenseNumberIsInvalid_ReturnsBadRequest()
    {
        // Arrange
        var request = new ProviderCredentialingController.StartCredentialingRequest(
            LicenseNumber: "INVALID@#", // Contains invalid characters
            MedicalBoard: "Medical Board of California",
            ExpiryDate: FixedFutureDate,
            FirstName: "John",
            LastName: "Doe",
            Email: "john.doe@example.com",
            Specialty: "Cardiology"
        );

        var controller = new ProviderCredentialingController(
            _mockTemporalClient.Object,
            _mockPendingReviewsQuery.Object,
            _timeProvider
        );

        // Act
        var result = await controller.StartCredentialing(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().Be("License number must contain only alphanumeric characters and hyphens");
    }

    [Fact]
    public async Task StartCredentialing_WhenExpiryDateIsDefault_ReturnsBadRequest()
    {
        // Arrange
        var request = new ProviderCredentialingController.StartCredentialingRequest(
            LicenseNumber: "LICENSE123456",
            MedicalBoard: "Medical Board of California",
            ExpiryDate: default(DateOnly),
            FirstName: "John",
            LastName: "Doe",
            Email: "john.doe@example.com",
            Specialty: "Cardiology"
        );

        var controller = new ProviderCredentialingController(
            _mockTemporalClient.Object,
            _mockPendingReviewsQuery.Object,
            _timeProvider
        );

        // Act
        var result = await controller.StartCredentialing(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult!.Value.Should().Be("Expiry date cannot be default");
    }
    #endregion

    #region GetPendingReviews Tests
    [Fact]
    public async Task GetPendingReviews_WhenCalled_ReturnsOk()
    {
        // Arrange
        var reviews = new List<PendingManualReviewDto>();
        _mockPendingReviewsQuery
            .Setup(x => x.GetPendingReviewsAsync())
            .ReturnsAsync(reviews);

        var controller = new ProviderCredentialingController(
            _mockTemporalClient.Object,
            _mockPendingReviewsQuery.Object,
            _timeProvider
        );

        // Act
        var result = await controller.GetPendingReviews();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(reviews);
    }
    #endregion
}
