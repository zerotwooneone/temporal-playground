namespace TemporalDDD.Domain.TimesheetProcessing;

public interface ITimesheetRepository
{
    Task<Timesheet?> GetByIdAsync(TimesheetId id, CancellationToken cancellationToken = default);
    Task SaveAsync(Timesheet aggregate, CancellationToken cancellationToken = default);
}
