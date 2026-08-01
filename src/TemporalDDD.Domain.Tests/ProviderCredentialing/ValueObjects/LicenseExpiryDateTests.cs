using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.Testing;

namespace TemporalDDD.Domain.Tests.ProviderCredentialing.ValueObjects;

public class LicenseExpiryDateTests
{
    private static readonly DateTimeOffset FixedCurrentDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    #region Create Tests
    [Fact]
    public void Create_WhenDateIsInFuture_ReturnsSuccess()
    {
        // Arrange
        var timeProvider = new FixedTimeProvider(FixedCurrentDate);
        var futureDate = new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Act
        var result = LicenseExpiryDate.Create(futureDate, timeProvider);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Value.Should().Be(futureDate);
    }

    [Fact]
    public void Create_WhenDateIsInPast_ReturnsFailure()
    {
        // Arrange
        var timeProvider = new FixedTimeProvider(FixedCurrentDate);
        var pastDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        // Act
        var result = LicenseExpiryDate.Create(pastDate, timeProvider);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("License expiry date must be in the future");
    }

    [Fact]
    public void Create_WhenDateIsCurrent_ReturnsFailure()
    {
        // Arrange
        var timeProvider = new FixedTimeProvider(FixedCurrentDate);
        var currentDate = FixedCurrentDate;

        // Act
        var result = LicenseExpiryDate.Create(currentDate, timeProvider);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("License expiry date must be in the future");
    }
    #endregion

    #region IsExpired Tests
    [Fact]
    public void IsExpired_WhenExpiryDateIsInPast_ReturnsTrue()
    {
        // Arrange
        // Create with a time provider where the date is in the future
        var creationTimeProvider = new FixedTimeProvider(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var expiryDate = LicenseExpiryDate.Create(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), creationTimeProvider).Value!;
        
        // Check with a time provider where the date is in the past
        var checkTimeProvider = new FixedTimeProvider(FixedCurrentDate);

        // Act
        var isExpired = expiryDate.IsExpired(checkTimeProvider.UtcNow);

        // Assert
        isExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenExpiryDateIsInFuture_ReturnsFalse()
    {
        // Arrange
        var timeProvider = new FixedTimeProvider(FixedCurrentDate);
        var expiryDate = LicenseExpiryDate.Create(new DateTimeOffset(2027, 1, 1, 0, 0, 0, TimeSpan.Zero), timeProvider).Value!;

        // Act
        var isExpired = expiryDate.IsExpired(timeProvider.UtcNow);

        // Assert
        isExpired.Should().BeFalse();
    }
    #endregion

    #region DaysUntilExpiry Tests
    [Fact]
    public void DaysUntilExpiry_WhenDateIsInFuture_ReturnsCorrectDays()
    {
        // Arrange
        var timeProvider = new FixedTimeProvider(FixedCurrentDate);
        var expiryDate = LicenseExpiryDate.Create(new DateTimeOffset(2026, 1, 11, 0, 0, 0, TimeSpan.Zero), timeProvider).Value!;

        // Act
        var daysUntilExpiry = expiryDate.DaysUntilExpiry(timeProvider.UtcNow);

        // Assert
        daysUntilExpiry.Should().Be(10);
    }

    [Fact]
    public void DaysUntilExpiry_WhenDateIsInPast_ReturnsZero()
    {
        // Arrange
        // Create with a time provider where the date is in the future
        var creationTimeProvider = new FixedTimeProvider(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var expiryDate = LicenseExpiryDate.Create(new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero), creationTimeProvider).Value!;
        
        // Check with a time provider where the date is in the past
        var checkTimeProvider = new FixedTimeProvider(FixedCurrentDate);

        // Act
        var daysUntilExpiry = expiryDate.DaysUntilExpiry(checkTimeProvider.UtcNow);

        // Assert
        daysUntilExpiry.Should().Be(0);
    }
    #endregion
}
