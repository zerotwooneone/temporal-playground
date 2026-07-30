using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.ProviderCredentialing;

public sealed record ProviderProfileId
{
    public uint Value { get; }

    private ProviderProfileId(uint value) => Value = value;

    public static Result<ProviderProfileId> Create(uint value)
    {
        if (value == 0)
            return Result<ProviderProfileId>.Failure("ProviderProfileId cannot be zero");
        return Result<ProviderProfileId>.Success(new ProviderProfileId(value));
    }

    public static implicit operator uint(ProviderProfileId id) => id.Value;
    public override string ToString() => Value.ToString();
}
