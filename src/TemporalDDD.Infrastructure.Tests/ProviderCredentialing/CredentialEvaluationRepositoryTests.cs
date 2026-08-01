using Microsoft.EntityFrameworkCore;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Domain.Testing;
using TemporalDDD.Infrastructure.Persistence;
using TemporalDDD.Infrastructure.ProviderCredentialing;

namespace TemporalDDD.Infrastructure.Tests.ProviderCredentialing;

public class CredentialEvaluationRepositoryTests
{
    private static readonly DateOnly FixedCurrentDate = new DateOnly(2026, 1, 1);
    private static readonly DateOnly FixedExpiryDate = new DateOnly(2028, 1, 1);
    private readonly ITimeProvider _timeProvider;

    public CredentialEvaluationRepositoryTests()
    {
        _timeProvider = new FixedTimeProvider(new DateTimeOffset(FixedCurrentDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero));
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
        var repository = new CredentialEvaluationRepository(dbContext, _timeProvider);
        
        var providerId = ProviderId.New();
        var publicId = CredentialEvaluationPublicId.New();
        var licenseNumber = LicenseNumber.Create("LICENSE123456").Value!;
        var medicalBoard = MedicalBoard.Create("Medical Board of California").Value!;
        var licenseExpiryDate = LicenseExpiryDate.Create(FixedExpiryDate).Value!;
        
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
        var repository = new CredentialEvaluationRepository(dbContext, _timeProvider);
        
        var providerId = ProviderId.New();
        var publicId = CredentialEvaluationPublicId.New();
        var licenseNumber = LicenseNumber.Create("LICENSE123456").Value!;
        var medicalBoard = MedicalBoard.Create("Medical Board of California").Value!;
        var licenseExpiryDate = LicenseExpiryDate.Create(FixedExpiryDate).Value!;
        
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
        var repository = new CredentialEvaluationRepository(dbContext, _timeProvider);
        
        var providerId = ProviderId.New();
        var publicId = CredentialEvaluationPublicId.New();
        var licenseNumber = LicenseNumber.Create("LICENSE123456").Value!;
        var medicalBoard = MedicalBoard.Create("Medical Board of California").Value!;
        var licenseExpiryDate = LicenseExpiryDate.Create(FixedExpiryDate).Value!;
        
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
        var repository = new CredentialEvaluationRepository(dbContext, _timeProvider);
        var nonExistentId = CredentialEvaluationId.New();

        // Act
        var retrievedEvaluation = await repository.GetByIdAsync(nonExistentId);

        // Assert
        retrievedEvaluation.Should().BeNull();
    }
    #endregion
}
