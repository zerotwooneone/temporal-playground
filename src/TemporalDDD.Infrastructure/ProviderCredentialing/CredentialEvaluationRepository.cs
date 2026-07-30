using Microsoft.EntityFrameworkCore;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.ProviderCredentialing;

public class CredentialEvaluationRepository : ICredentialEvaluationRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CredentialEvaluationRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CredentialEvaluation?> GetByIdAsync(CredentialEvaluationId id, CancellationToken cancellationToken = default)
    {
        var dbo = await _dbContext.CredentialEvaluations
            .FirstOrDefaultAsync(e => e.Id == id.Value, cancellationToken);
        
        if (dbo == null) return null;
        
        return MapToDomain(dbo);
    }

    public async Task SaveAsync(CredentialEvaluation aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.CredentialEvaluations
            .FirstOrDefaultAsync(e => e.Id == aggregate.Id.Value, cancellationToken);

        var dbo = MapToDbo(aggregate);

        if (existing == null)
        {
            _dbContext.CredentialEvaluations.Add(dbo);
        }
        else
        {
            _dbContext.Entry(existing).State = EntityState.Detached;
            _dbContext.CredentialEvaluations.Attach(dbo);
            _dbContext.Entry(dbo).State = EntityState.Modified;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private CredentialEvaluation MapToDomain(CredentialEvaluationDbo dbo)
    {
        var providerId = ProviderId.Create(dbo.ProviderId).Value ?? throw new InvalidOperationException($"Invalid provider ID in database: {dbo.ProviderId}");
        var licenseNumber = LicenseNumber.Create(dbo.LicenseNumber).Value ?? throw new InvalidOperationException($"Invalid license number in database: {dbo.LicenseNumber}");
        var medicalBoard = MedicalBoard.Create(dbo.MedicalBoard).Value ?? throw new InvalidOperationException($"Invalid medical board in database: {dbo.MedicalBoard}");
        var licenseExpiryDate = LicenseExpiryDate.Create(dbo.LicenseExpiryDate).Value ?? throw new InvalidOperationException($"Invalid license expiry date in database: {dbo.LicenseExpiryDate}");
        var complianceNotes = ComplianceNotes.Create(dbo.ComplianceNotes).Value ?? throw new InvalidOperationException($"Invalid compliance notes in database: {dbo.ComplianceNotes}");
        var statusResult = EvaluationStatus.FromValue(dbo.Status);
        if (statusResult.IsFailure)
            throw new InvalidOperationException($"Invalid EvaluationStatus in database: {dbo.Status}. {statusResult.Error}");
        var status = statusResult.Value;
        var evaluatedAt = DateTimeOffset.FromUnixTimeMilliseconds(dbo.EvaluatedAt);

        CredentialEvaluationPublicId? publicId = null;
        if (!string.IsNullOrEmpty(dbo.PublicId))
        {
            publicId = CredentialEvaluationPublicId.FromString(dbo.PublicId);
        }

        // Use internal constructor for rehydration (infrastructure concern)
        return new CredentialEvaluation(
            id: CredentialEvaluationId.Create(dbo.Id).Value ?? throw new InvalidOperationException($"Invalid credential evaluation ID in database: {dbo.Id}"),
            publicId: publicId,
            providerId: providerId,
            licenseNumber: licenseNumber,
            medicalBoard: medicalBoard,
            licenseExpiryDate: licenseExpiryDate,
            isCompliant: dbo.IsCompliant,
            complianceNotes: complianceNotes,
            evaluatedAt: evaluatedAt,
            status: status
        );
    }

    private CredentialEvaluationDbo MapToDbo(CredentialEvaluation evaluation)
    {
        return new CredentialEvaluationDbo
        {
            Id = evaluation.Id.Value,
            PublicId = evaluation.PublicId?.ToString(),
            ProviderId = evaluation.ProviderId.Value,
            LicenseNumber = evaluation.LicenseNumber.Value,
            MedicalBoard = evaluation.MedicalBoard.Value,
            LicenseExpiryDate = evaluation.LicenseExpiryDate.Value,
            IsCompliant = evaluation.IsCompliant,
            ComplianceNotes = evaluation.ComplianceNotes.Value,
            EvaluatedAt = evaluation.EvaluatedAt.ToUnixTimeMilliseconds(),
            Status = evaluation.Status.Value
        };
    }
}
