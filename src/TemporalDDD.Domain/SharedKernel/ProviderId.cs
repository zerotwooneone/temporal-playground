namespace TemporalDDD.Domain.SharedKernel;

public sealed record ProviderId
{
    public Guid Value { get; }

    private ProviderId(Guid value)
    {
        Value = value;
    }

    public static ProviderId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ProviderId cannot be empty", nameof(value));

        return new ProviderId(value);
    }

    public static ProviderId New() => new(Guid.NewGuid());

    public static implicit operator Guid(ProviderId id) => id.Value;

    public override string ToString() => Value.ToString();
}
