namespace TemporalDDD.Domain.PlacementMatching.ValueObjects;

public sealed record MatchScore
{
    public decimal Value { get; }

    private MatchScore(decimal value)
    {
        Value = value;
    }

    public static MatchScore Create(decimal value)
    {
        if (value < 0.0m || value > 100.0m)
            throw new ArgumentException("Match score must be between 0.0 and 100.0", nameof(value));

        return new MatchScore(value);
    }

    public static MatchScore Zero() => new(0.0m);
    public static MatchScore Perfect() => new(100.0m);

    public bool IsHighMatch() => Value >= 80.0m;
    public bool IsMediumMatch() => Value >= 60.0m && Value < 80.0m;
    public bool IsLowMatch() => Value < 60.0m;

    public static implicit operator decimal(MatchScore matchScore) => matchScore.Value;

    public override string ToString() => $"{Value:F2}%";
}
