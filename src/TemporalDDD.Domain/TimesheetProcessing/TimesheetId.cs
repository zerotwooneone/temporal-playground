using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.TimesheetProcessing;

public sealed record TimesheetId
{
    public uint Value { get; }

    private TimesheetId(uint value) => Value = value;

    public static Result<TimesheetId> Create(uint value)
    {
        if (value == 0)
            return Result<TimesheetId>.Failure("TimesheetId cannot be zero");
        return Result<TimesheetId>.Success(new TimesheetId(value));
    }

    public static implicit operator uint(TimesheetId id) => id.Value;
    public override string ToString() => Value.ToString();
}
