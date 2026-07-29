using Microsoft.EntityFrameworkCore;
using Temporalio.Activities;
using TemporalDDD.Application.ProviderCredentialing;
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
        var expiryDate = DateTime.UtcNow.AddYears(2);

        return new MedicalBoardLicenseInfo(
            LicenseNumber: licenseNumber,
            MedicalBoard: medicalBoard,
            ExpiryDate: expiryDate,
            IsValid: isValid,
            Notes: isValid ? "License verified successfully" : "License number format invalid"
        );
    }

    public async Task EvaluateAndSaveComplianceAsync(Guid evaluationId, MedicalBoardLicenseInfo licenseInfo)
    {
        // Simulate business rule evaluation
        var isCompliant = licenseInfo.IsValid && licenseInfo.ExpiryDate > DateTime.UtcNow.AddMonths(6);

        // Create domain entity
        var evaluation = new Domain.ProviderCredentialing.CredentialEvaluation(
            providerId: Guid.NewGuid(), // In real scenario, this would be passed as parameter
            licenseNumber: licenseInfo.LicenseNumber,
            medicalBoard: licenseInfo.MedicalBoard,
            licenseExpiryDate: licenseInfo.ExpiryDate
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

    public async Task RequestManualReviewAsync(Guid evaluationId)
    {
        // Simulate external notification (e.g., email, webhook)
        await Task.Delay(500);

        // In real implementation, this would send notification to compliance team
        Console.WriteLine($"[ManualReview] Request sent for evaluation {evaluationId}");
    }

    public async Task ActivateProviderProfileAsync(Guid providerId)
    {
        // Simulate database operation to activate provider
        await _dbContext.Database.EnsureCreatedAsync();
        
        // Note: DbSet would be added to ApplicationDbContext once schema is defined
        // For now, we simulate the activation
        await Task.Delay(100);

        Console.WriteLine($"[ProviderActivation] Provider {providerId} activated successfully");
    }
}
