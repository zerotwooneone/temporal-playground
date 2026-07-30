namespace TemporalDDD.Domain.SharedKernel;

public sealed record DateRange
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }

    private DateRange(DateTimeOffset start, DateTimeOffset end)
    {
        Start = start;
        End = end;
    }

    public static DateRange Create(DateTimeOffset start, DateTimeOffset end)
    {
        if (end < start)
            throw new ArgumentException("End date must be greater than or equal to start date", nameof(end));

        return new DateRange(start, end);
    }

    public TimeSpan Duration => End - Start;

    public int Days => Duration.Days;

    public bool Contains(DateTimeOffset date) => date >= Start && date <= End;

    public bool Overlaps(DateRange other) => Start <= other.End && End >= other.Start;

    public override string ToString() => $"{Start:yyyy-MM-dd} to {End:yyyy-MM-dd}";
}
