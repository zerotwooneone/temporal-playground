using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderCredentialing;

public sealed record CredentialEvaluationId
{
    private const string Abbreviation = "CRE";
    public Guid Value { get; }

    private CredentialEvaluationId(Guid value) => Value = value;

    public static Result<CredentialEvaluationId> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Result<CredentialEvaluationId>.Failure("CredentialEvaluation ID cannot be null or empty");

        var expectedPrefix = $"{Abbreviation}Id";
        if (!value.StartsWith(expectedPrefix))
            return Result<CredentialEvaluationId>.Failure($"CredentialEvaluation ID must start with '{expectedPrefix}'");

        var guidString = value.Substring(expectedPrefix.Length);
        if (!Guid.TryParse(guidString, out var guid))
            return Result<CredentialEvaluationId>.Failure("Invalid GUID format in CredentialEvaluation ID");

        return Result<CredentialEvaluationId>.Success(new CredentialEvaluationId(guid));
    }

    public static CredentialEvaluationId New() => new CredentialEvaluationId(Guid.CreateVersion7());

    public override string ToString() => $"{Abbreviation}Id{Value}";
}
