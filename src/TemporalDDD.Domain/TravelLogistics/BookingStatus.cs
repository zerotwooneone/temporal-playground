namespace TemporalDDD.Domain.TravelLogistics;

public sealed record BookingStatus
{
    public int Value { get; }
    public string Name { get; }

    private BookingStatus(int value, string name)
    {
        Value = value;
        Name = name;
    }

    public static readonly BookingStatus Pending = new(0, "Pending");
    public static readonly BookingStatus Confirmed = new(1, "Confirmed");
    public static readonly BookingStatus Cancelled = new(2, "Cancelled");

    private static readonly BookingStatus[] AllStatuses = { Pending, Confirmed, Cancelled };

    public static BookingStatus FromValue(int value)
    {
        return AllStatuses.FirstOrDefault(s => s.Value == value) 
            ?? throw new ArgumentException($"Invalid BookingStatus value: {value}", nameof(value));
    }

    public static implicit operator int(BookingStatus status) => status.Value;

    public override string ToString() => Name;
}
