namespace TemporalDDD.Domain.SharedKernel;

public interface ITimeProvider
{
    DateTimeOffset UtcNow { get; }
    DateOnly LocalToday { get; }
}
