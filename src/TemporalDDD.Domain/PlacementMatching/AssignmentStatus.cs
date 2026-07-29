namespace TemporalDDD.Domain.PlacementMatching;

public sealed record AssignmentStatus
{
    public int Value { get; }
    public string Name { get; }

    private AssignmentStatus(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public static readonly AssignmentStatus Proposed = new(0, "Proposed");
    public static readonly AssignmentStatus Accepted = new(1, "Accepted");
    public static readonly AssignmentStatus Rejected = new(2, "Rejected");
    public static readonly AssignmentStatus Revoked = new(3, "Revoked");

    private static readonly AssignmentStatus[] AllStatuses = { Proposed, Accepted, Rejected, Revoked };

    public static AssignmentStatus FromValue(int value)
    {
        return AllStatuses.FirstOrDefault(s => s.Value == value) 
            ?? throw new ArgumentException($"Invalid AssignmentStatus value: {value}", nameof(value));
    }

    public static implicit operator int(AssignmentStatus status) => status.Value;

    public override string ToString() => Name;
}
