using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Domain.Testing;

public sealed class FixedTimeProvider : ITimeProvider
{
    public DateTimeOffset UtcNow { get; }
    public DateOnly LocalToday { get; }

    public FixedTimeProvider(DateTimeOffset fixedTime)
    {
        UtcNow = fixedTime.ToUniversalTime();
        LocalToday = DateOnly.FromDateTime(fixedTime.ToLocalTime().Date);
    }
}
