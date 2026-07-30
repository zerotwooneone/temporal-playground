namespace TemporalDDD.Domain.PlacementMatching;

public sealed record AssignmentPublicId
{
    private const string Prefix = "ASN";
    public Guid Value { get; }

    private AssignmentPublicId(Guid value)
    {
        Value = value;
    }

    public static AssignmentPublicId Create(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("AssignmentPublicId cannot be empty", nameof(value));

        return new AssignmentPublicId(value);
    }

    public static AssignmentPublicId New() => new(Guid.NewGuid());

    public static implicit operator Guid(AssignmentPublicId id) => id.Value;

    public override string ToString() => $"{Prefix}_{Value:N}";
}
