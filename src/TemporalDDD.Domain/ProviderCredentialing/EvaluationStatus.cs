namespace TemporalDDD.Domain.ProviderCredentialing;

public sealed record EvaluationStatus
{
    public int Value { get; }
    public string Name { get; }

    private EvaluationStatus(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public static readonly EvaluationStatus Pending = new(0, "Pending");
    public static readonly EvaluationStatus Approved = new(1, "Approved");
    public static readonly EvaluationStatus Rejected = new(2, "Rejected");
    public static readonly EvaluationStatus ManualReviewRequired = new(3, "ManualReviewRequired");

    private static readonly EvaluationStatus[] AllStatuses = { Pending, Approved, Rejected, ManualReviewRequired };

    public static EvaluationStatus FromValue(int value)
    {
        return AllStatuses.FirstOrDefault(s => s.Value == value) 
            ?? throw new ArgumentException($"Invalid EvaluationStatus value: {value}", nameof(value));
    }

    public static implicit operator int(EvaluationStatus status) => status.Value;

    public override string ToString() => Name;
}
