using TemporalDDD.Domain.SharedKernel;

namespace TemporalDDD.Infrastructure.SharedKernel;

public sealed class SystemTimeProvider : ITimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
