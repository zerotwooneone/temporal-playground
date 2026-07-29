namespace TemporalDDD.Domain.SharedKernel;

public sealed record ProviderId
{
    public uint Value { get; }

    private ProviderId(uint value)
    {
        Value = value;
    }

    public static ProviderId Create(uint value)
    {
        if (value == 0)
            throw new ArgumentException("ProviderId cannot be zero", nameof(value));

        return new ProviderId(value);
    }

    // Factory method for rehydration from database
    public static ProviderId FromDatabase(uint value) => new(value);

    public static implicit operator uint(ProviderId id) => id.Value;

    public override string ToString() => Value.ToString();
}
