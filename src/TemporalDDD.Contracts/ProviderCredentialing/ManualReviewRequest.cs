namespace TemporalDDD.Contracts.ProviderCredentialing;

public sealed record ManualReviewRequest(
    bool IsApproved,
    string? Notes);
