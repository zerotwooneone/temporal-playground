using Microsoft.EntityFrameworkCore;
using Moq;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Infrastructure.Persistence;
using TemporalDDD.Infrastructure.ProviderCredentialing;

namespace TemporalDDD.Infrastructure.Tests.ProviderCredentialing;

public class CredentialEvaluationRepositoryTests
{
    private static readonly DateTimeOffset FixedCurrentDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset FixedExpiryDate = new DateTimeOffset(2028, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private readonly Mock<ITimeProvider> _mockTimeProvider;

    public CredentialEvaluationRepositoryTests()
    {
        _mockTimeProvider = new Mock<ITimeProvider>();
        _mockTimeProvider.Setup(x => x.UtcNow).Returns(FixedCurrentDate);
    }

    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    #region SaveAsync Tests
    [Fact]
    public async Task SaveAsync_WhenEvaluationIsNew_SavesToDatabase()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var repository = new CredentialEvaluationRepository(dbContext, _mockTimeProvider.Object);
        
        var providerId = ProviderId.New();
        var publicId = CredentialEvaluationPublicId.New();
        var licenseNumber = LicenseNumber.Create("LICENSE123456").Value!;
        var medicalBoard = MedicalBoard.Create("Medical Board of California").Value!;
        var licenseExpiryDate = LicenseExpiryDate.Create(FixedExpiryDate, _mockTimeProvider.Object).Value!;
        
        var evaluation = CredentialEvaluation.Create(
            providerId,
            publicId,
            licenseNumber,
            medicalBoard,
            licenseExpiryDate
        );

        // Act
        await repository.SaveAsync(evaluation);

        // Assert
        var savedEvaluation = await repository.GetByIdAsync(evaluation.Id);
        savedEvaluation.Should().NotBeNull();
        savedEvaluation!.Id.Should().Be(evaluation.Id);
        savedEvaluation.ProviderId.Should().Be(evaluation.ProviderId);
        savedEvaluation.LicenseNumber.Should().Be(evaluation.LicenseNumber);
    }

    [Fact]
    public async Task SaveAsync_WhenEvaluationExists_UpdatesDatabase()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var repository = new CredentialEvaluationRepository(dbContext, _mockTimeProvider.Object);
        
        var providerId = ProviderId.New();
        var publicId = CredentialEvaluationPublicId.New();
        var licenseNumber = LicenseNumber.Create("LICENSE123456").Value!;
        var medicalBoard = MedicalBoard.Create("Medical Board of California").Value!;
        var licenseExpiryDate = LicenseExpiryDate.Create(FixedExpiryDate, _mockTimeProvider.Object).Value!;
        
        var evaluation = CredentialEvaluation.Create(
            providerId,
            publicId,
            licenseNumber,
            medicalBoard,
            licenseExpiryDate
        );
        
        await repository.SaveAsync(evaluation);
        
        // Modify the evaluation
        evaluation.MarkAsCompliant("Updated compliance notes");
        
        // Act
        await repository.SaveAsync(evaluation);

        // Assert
        var updatedEvaluation = await repository.GetByIdAsync(evaluation.Id);
        updatedEvaluation.Should().NotBeNull();
        updatedEvaluation!.IsCompliant.Should().BeTrue();
        updatedEvaluation.ComplianceNotes.Value.Should().Be("Updated compliance notes");
    }
    #endregion

    #region GetByIdAsync Tests
    [Fact]
    public async Task GetByIdAsync_WhenEvaluationExists_ReturnsEvaluation()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var repository = new CredentialEvaluationRepository(dbContext, _mockTimeProvider.Object);
        
        var providerId = ProviderId.New();
        var publicId = CredentialEvaluationPublicId.New();
        var licenseNumber = LicenseNumber.Create("LICENSE123456").Value!;
        var medicalBoard = MedicalBoard.Create("Medical Board of California").Value!;
        var licenseExpiryDate = LicenseExpiryDate.Create(FixedExpiryDate, _mockTimeProvider.Object).Value!;
        
        var evaluation = CredentialEvaluation.Create(
            providerId,
            publicId,
            licenseNumber,
            medicalBoard,
            licenseExpiryDate
        );
        
        await repository.SaveAsync(evaluation);

        // Act
        var retrievedEvaluation = await repository.GetByIdAsync(evaluation.Id);

        // Assert
        retrievedEvaluation.Should().NotBeNull();
        retrievedEvaluation!.Id.Should().Be(evaluation.Id);
        retrievedEvaluation.ProviderId.Should().Be(evaluation.ProviderId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenEvaluationDoesNotExist_ReturnsNull()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var repository = new CredentialEvaluationRepository(dbContext, _mockTimeProvider.Object);
        var nonExistentId = CredentialEvaluationId.New();

        // Act
        var retrievedEvaluation = await repository.GetByIdAsync(nonExistentId);

        // Assert
        retrievedEvaluation.Should().BeNull();
    }
    #endregion
}
