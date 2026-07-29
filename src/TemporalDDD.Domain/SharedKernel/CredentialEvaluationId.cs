namespace TemporalDDD.Domain.SharedKernel;

public sealed record CredentialEvaluationId
{
    public uint Value { get; }

    private CredentialEvaluationId(uint value) => Value = value;

    public static CredentialEvaluationId Create(uint value)
    {
        if (value == 0)
            throw new ArgumentException("CredentialEvaluationId cannot be zero", nameof(value));
        return new CredentialEvaluationId(value);
    }

    public static CredentialEvaluationId FromDatabase(uint value) => new(value);
    public static implicit operator uint(CredentialEvaluationId id) => id.Value;
    public override string ToString() => Value.ToString();
}
