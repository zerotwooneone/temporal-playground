namespace TemporalDDD.Domain.SharedKernel;

public sealed record FacilityId
{
    public Guid Value { get; }

    private FacilityId(Guid value)
    {
        Value = value;
    }

    public static FacilityId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("FacilityId cannot be empty", nameof(value));

        return new FacilityId(value);
    }

    public static FacilityId New() => new(Guid.NewGuid());

    public static implicit operator Guid(FacilityId id) => id.Value;

    public override string ToString() => Value.ToString();
}
