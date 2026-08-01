using Microsoft.EntityFrameworkCore;
using TemporalDDD.Domain.ProviderCredentialing;
using TemporalDDD.Domain.ProviderCredentialing.ValueObjects;
using TemporalDDD.Domain.SharedKernel;
using TemporalDDD.Infrastructure.Persistence;

namespace TemporalDDD.Infrastructure.ProviderCredentialing;

public class CredentialEvaluationRepository : ICredentialEvaluationRepository
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ITimeProvider _timeProvider;

    public CredentialEvaluationRepository(ApplicationDbContext dbContext, ITimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<CredentialEvaluation?> GetByIdAsync(CredentialEvaluationId id, CancellationToken cancellationToken = default)
    {
        var dbo = await _dbContext.CredentialEvaluations
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id.ToString(), cancellationToken);

        if (dbo == null) return null;

        return MapToDomain(dbo);
    }

    public async Task SaveAsync(CredentialEvaluation aggregate, CancellationToken cancellationToken = default)
    {
        var id = aggregate.Id.ToString();
        var existing = await _dbContext.CredentialEvaluations
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (existing == null)
        {
            existing = new CredentialEvaluationDbo();
            MapToDbo(aggregate, existing);
            _dbContext.CredentialEvaluations.Add(existing);
        }
        else
        {
            MapToDbo(aggregate, existing);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private CredentialEvaluation MapToDomain(CredentialEvaluationDbo dbo)
    {
        var providerId = ProviderId.Create(dbo.ProviderId).Value ?? throw new InvalidOperationException($"Invalid provider ID in database: {dbo.ProviderId}");
        var licenseNumber = LicenseNumber.Create(dbo.LicenseNumber).Value ?? throw new InvalidOperationException($"Invalid license number in database: {dbo.LicenseNumber}");
        var medicalBoard = MedicalBoard.Create(dbo.MedicalBoard).Value ?? throw new InvalidOperationException($"Invalid medical board in database: {dbo.MedicalBoard}");
        var licenseExpiryDate = new LicenseExpiryDate(dbo.LicenseExpiryDate);
        var complianceNotes = ComplianceNotes.Create(dbo.ComplianceNotes).Value ?? throw new InvalidOperationException($"Invalid compliance notes in database: {dbo.ComplianceNotes}");
        var statusResult = EvaluationStatus.FromValue(dbo.Status);
        if (statusResult.IsFailure)
            throw new InvalidOperationException($"Invalid EvaluationStatus in database: {dbo.Status}. {statusResult.Error}");
        var status = statusResult.Value;
        var evaluatedAt = DateTimeOffset.FromUnixTimeMilliseconds(dbo.EvaluatedAt);

        var publicId =  CredentialEvaluationPublicId.Create(dbo.PublicId).Value ?? throw new InvalidOperationException($"Invalid public ID in database: {dbo.PublicId}");

        var id = CredentialEvaluationId.Create(dbo.Id).Value ?? throw new InvalidOperationException($"Invalid credential evaluation ID in database: {dbo.Id}");

        // Use internal constructor for rehydration (infrastructure concern)
        return new CredentialEvaluation(
            id: id,
            publicId: publicId,
            providerId: providerId,
            licenseNumber: licenseNumber,
            medicalBoard: medicalBoard,
            licenseExpiryDate: licenseExpiryDate,
            isCompliant: dbo.IsCompliant,
            complianceNotes: complianceNotes,
            evaluatedAt: evaluatedAt,
            status: status,
            workflowId: dbo.WorkflowId
        );
    }

    private void MapToDbo(CredentialEvaluation evaluation, CredentialEvaluationDbo dbo)
    {
        dbo.Id = evaluation.Id.ToString();
        dbo.PublicId = evaluation.PublicId.ToString();
        dbo.ProviderId = evaluation.ProviderId.ToString();
        dbo.LicenseNumber = evaluation.LicenseNumber.Value;
        dbo.MedicalBoard = evaluation.MedicalBoard.Value;
        dbo.LicenseExpiryDate = evaluation.LicenseExpiryDate.Value;
        dbo.IsCompliant = evaluation.IsCompliant;
        dbo.ComplianceNotes = evaluation.ComplianceNotes.Value;
        dbo.EvaluatedAt = evaluation.EvaluatedAt.ToUnixTimeMilliseconds();
        dbo.Status = evaluation.Status.Value;
        dbo.WorkflowId = evaluation.WorkflowId;
    }
}
