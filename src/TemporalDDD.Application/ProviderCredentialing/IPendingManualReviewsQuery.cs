namespace TemporalDDD.Application.ProviderCredentialing;

public interface IPendingManualReviewsQuery
{
    Task<List<PendingManualReviewDto>> GetPendingReviewsAsync(CancellationToken cancellationToken = default);
}

public record PendingManualReviewDto(
    string EvaluationId,
    string ProviderId,
    string LicenseNumber,
    string MedicalBoard,
    DateTimeOffset EvaluatedAt,
    string WorkflowId
);
