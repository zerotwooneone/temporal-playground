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

    public static AssignmentPublicId FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("AssignmentPublicId cannot be null or whitespace", nameof(value));

        var parts = value.Split('_');
        if (parts.Length != 2)
            throw new ArgumentException("AssignmentPublicId must be in format 'PREFIX_Guid'", nameof(value));

        if (parts[0] != Prefix)
            throw new ArgumentException($"AssignmentPublicId must have prefix '{Prefix}'", nameof(value));

        var guidValue = Guid.Parse(parts[1]);
        if (guidValue == Guid.Empty)
            throw new ArgumentException("AssignmentPublicId cannot be empty", nameof(value));

        return new AssignmentPublicId(guidValue);
    }

    public static AssignmentPublicId New() => new(Guid.NewGuid());

    public static implicit operator Guid(AssignmentPublicId id) => id.Value;

    public override string ToString() => $"{Prefix}_{Value:N}";
}
