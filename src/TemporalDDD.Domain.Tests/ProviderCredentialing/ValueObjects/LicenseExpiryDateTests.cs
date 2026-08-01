using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;

namespace TemporalDDD.Domain.Tests.ProviderCredentialing.ValueObjects;

public class LicenseExpiryDateTests
{
    private static readonly DateOnly FixedCurrentDate = new DateOnly(2026, 1, 1);

    #region Create Tests
    [Fact]
    public void Create_WhenDateIsValid_ReturnsSuccess()
    {
        // Arrange
        var futureDate = new DateOnly(2027, 1, 1);

        // Act
        var result = LicenseExpiryDate.Create(futureDate);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Value.Should().Be(futureDate);
    }

    [Fact]
    public void Create_WhenDateIsDefault_ReturnsFailure()
    {
        // Arrange
        var defaultDate = default(DateOnly);

        // Act
        var result = LicenseExpiryDate.Create(defaultDate);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Expiry date cannot be default");
    }
    #endregion

    #region IsExpired Tests
    [Fact]
    public void IsExpired_WhenExpiryDateIsInPast_ReturnsTrue()
    {
        // Arrange
        var expiryDate = LicenseExpiryDate.Create(new DateOnly(2025, 1, 1)).Value!;
        var today = FixedCurrentDate;

        // Act
        var isExpired = expiryDate.IsExpired(today);

        // Assert
        isExpired.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenExpiryDateIsInFuture_ReturnsFalse()
    {
        // Arrange
        var expiryDate = LicenseExpiryDate.Create(new DateOnly(2027, 1, 1)).Value!;
        var today = FixedCurrentDate;

        // Act
        var isExpired = expiryDate.IsExpired(today);

        // Assert
        isExpired.Should().BeFalse();
    }
    #endregion

    #region DaysUntilExpiry Tests
    [Fact]
    public void DaysUntilExpiry_WhenDateIsInFuture_ReturnsCorrectDays()
    {
        // Arrange
        var expiryDate = LicenseExpiryDate.Create(new DateOnly(2026, 1, 11)).Value!;
        var today = FixedCurrentDate;

        // Act
        var daysUntilExpiry = expiryDate.DaysUntilExpiry(today);

        // Assert
        daysUntilExpiry.Should().Be(10);
    }

    [Fact]
    public void DaysUntilExpiry_WhenDateIsInPast_ReturnsZero()
    {
        // Arrange
        var expiryDate = LicenseExpiryDate.Create(new DateOnly(2025, 1, 1)).Value!;
        var today = FixedCurrentDate;

        // Act
        var daysUntilExpiry = expiryDate.DaysUntilExpiry(today);

        // Assert
        daysUntilExpiry.Should().Be(0);
    }
    #endregion

    #region DaysSinceExpiry Tests
    [Fact]
    public void DaysSinceExpiry_WhenDateIsInPast_ReturnsCorrectDays()
    {
        // Arrange
        var expiryDate = LicenseExpiryDate.Create(new DateOnly(2025, 1, 1)).Value!;
        var today = FixedCurrentDate;

        // Act
        var daysSinceExpiry = expiryDate.DaysSinceExpiry(today);

        // Assert
        daysSinceExpiry.Should().Be(365); // 2025 is not a leap year
    }

    [Fact]
    public void DaysSinceExpiry_WhenDateIsInFuture_ReturnsZero()
    {
        // Arrange
        var expiryDate = LicenseExpiryDate.Create(new DateOnly(2027, 1, 1)).Value!;
        var today = FixedCurrentDate;

        // Act
        var daysSinceExpiry = expiryDate.DaysSinceExpiry(today);

        // Assert
        daysSinceExpiry.Should().Be(0);
    }
    #endregion
}
