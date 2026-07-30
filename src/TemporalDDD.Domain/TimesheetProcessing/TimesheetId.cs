using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.TimesheetProcessing;

public sealed record TimesheetId
{
    private const string Abbreviation = "TSH";
    public Guid Value { get; }

    private TimesheetId(Guid value) => Value = value;

    public static Result<TimesheetId> Create(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Result<TimesheetId>.Failure("Timesheet ID cannot be null or empty");

        var expectedPrefix = $"{Abbreviation}Id";
        if (!value.StartsWith(expectedPrefix))
            return Result<TimesheetId>.Failure($"Timesheet ID must start with '{expectedPrefix}'");

        var guidString = value.Substring(expectedPrefix.Length);
        if (!Guid.TryParse(guidString, out var guid))
            return Result<TimesheetId>.Failure("Invalid GUID format in Timesheet ID");

        return Result<TimesheetId>.Success(new TimesheetId(guid));
    }

    public static TimesheetId New() => new TimesheetId(Guid.CreateVersion7());

    public override string ToString() => $"{Abbreviation}Id{Value}";
}
