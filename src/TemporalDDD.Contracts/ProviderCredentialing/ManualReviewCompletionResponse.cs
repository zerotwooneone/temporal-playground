namespace TemporalDDD.Contracts.ProviderCredentialing;

public abstract record ManualReviewCompletionResponse
{
    public sealed record Success(string Message) : ManualReviewCompletionResponse;

    public sealed record WorkflowAlreadyComplete(string EvaluationPublicId, string Message) : ManualReviewCompletionResponse;

    public sealed record ValidationError(string Message) : ManualReviewCompletionResponse;

    public sealed record SystemError(string Message) : ManualReviewCompletionResponse;
}
