namespace TemporalDDD.Domain.SharedKernel;

public sealed record ProviderProfileId
{
    public uint Value { get; }

    private ProviderProfileId(uint value) => Value = value;

    public static ProviderProfileId Create(uint value)
    {
        if (value == 0)
            throw new ArgumentException("ProviderProfileId cannot be zero", nameof(value));
        return new ProviderProfileId(value);
    }

    public static ProviderProfileId FromDatabase(uint value) => new(value);
    public static implicit operator uint(ProviderProfileId id) => id.Value;
    public override string ToString() => Value.ToString();
}
