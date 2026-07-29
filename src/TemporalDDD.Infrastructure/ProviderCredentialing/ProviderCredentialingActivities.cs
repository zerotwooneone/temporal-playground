using Microsoft.EntityFrameworkCore;
using Temporalio.Activities;
using TemporalDDD.Application.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.ProviderCredentialing;

public class ProviderCredentialingActivities : IProviderCredentialingActivities
{
    private readonly ApplicationDbContext _dbContext;

    public ProviderCredentialingActivities(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<MedicalBoardLicenseInfo> FetchMedicalBoardLicenseAsync(string licenseNumber, string medicalBoard)
    {
        // Simulate external API call to medical board
        await Task.Delay(1000);

        // Simulated response - in real implementation, this would call actual medical board API
        var isValid = !string.IsNullOrEmpty(licenseNumber) && licenseNumber.Length >= 8;
        var expiryDate = DateTimeOffset.UtcNow.AddYears(2);

        return new MedicalBoardLicenseInfo(
            LicenseNumber: licenseNumber,
            MedicalBoard: medicalBoard,
            ExpiryDate: expiryDate,
            IsValid: isValid,
            ProviderId: 1, // Simulated provider ID
            Notes: isValid ? "License verified successfully" : "License number format invalid"
        );
    }

    public async Task EvaluateAndSaveComplianceAsync(uint evaluationId, MedicalBoardLicenseInfo licenseInfo)
    {
        // Simulate business rule evaluation
        var isCompliant = licenseInfo.IsValid && licenseInfo.ExpiryDate > DateTimeOffset.UtcNow.AddMonths(6);

        // Create value objects
        var licenseNumberVo = LicenseNumber.Create(licenseInfo.LicenseNumber);
        var medicalBoardVo = MedicalBoard.Create(licenseInfo.MedicalBoard);
        var licenseExpiryDateVo = LicenseExpiryDate.Create(licenseInfo.ExpiryDate);
        var providerIdVo = ProviderId.Create(licenseInfo.ProviderId);

        // Create domain entity using factory
        var evaluation = Domain.ProviderCredentialing.CredentialEvaluation.Create(
            providerId: providerIdVo,
            licenseNumber: licenseNumberVo,
            medicalBoard: medicalBoardVo,
            licenseExpiryDate: licenseExpiryDateVo
        );

        if (isCompliant)
        {
            evaluation.MarkAsCompliant(licenseInfo.Notes);
        }
        else
        {
            evaluation.RequestManualReview();
        }

        // Save to database (DB Write - separate from external API call)
        await _dbContext.Database.EnsureCreatedAsync();
        
        // Note: DbSet would be added to ApplicationDbContext once schema is defined
        // For now, we simulate the save
        await Task.Delay(100);
    }

    public async Task RequestManualReviewAsync(uint evaluationId)
    {
        // Simulate external notification (e.g., email, webhook)
        await Task.Delay(500);

        // In real implementation, this would send notification to compliance team
        Console.WriteLine($"[ManualReview] Request sent for evaluation {evaluationId}");
    }

    public async Task ActivateProviderProfileAsync(uint providerId)
    {
        // Simulate database operation to activate provider
        await _dbContext.Database.EnsureCreatedAsync();
        
        // Note: DbSet would be added to ApplicationDbContext once schema is defined
        // For now, we simulate the activation
        await Task.Delay(100);

        Console.WriteLine($"[ProviderActivation] Provider {providerId} activated successfully");
    }
}
