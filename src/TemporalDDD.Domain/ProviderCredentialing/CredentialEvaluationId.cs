using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderCredentialing;

public sealed record CredentialEvaluationId
{
    public uint Value { get; }

    private CredentialEvaluationId(uint value) => Value = value;

    public static Result<CredentialEvaluationId> Create(uint value)
    {
        if (value == 0)
            return Result<CredentialEvaluationId>.Failure("CredentialEvaluationId cannot be zero");
        return Result<CredentialEvaluationId>.Success(new CredentialEvaluationId(value));
    }

    public static implicit operator uint(CredentialEvaluationId id) => id.Value;
    public override string ToString() => Value.ToString();
}
