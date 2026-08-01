using Rebus.Bus;
using TemporalDDD.Application.Messaging;

namespace TemporalDDD.Infrastructure.Messaging;

public class RebusMessagePublisher : IMessagePublisher
{
    private readonly IBus _bus;

    public RebusMessagePublisher(IBus bus)
    {
        _bus = bus;
    }

    public async Task PublishEventAsync(
        IApplicationEvent message, 
        EventPublishOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        var rebusHeaders = new Dictionary<string, string>();

        if (options != null)
        {
            if (!string.IsNullOrEmpty(options.CorrelationId))
                rebusHeaders[Rebus.Messages.Headers.CorrelationId] = options.CorrelationId;

            if (options.Headers != null)
            {
                foreach (var kvp in options.Headers)
                    rebusHeaders[kvp.Key] = kvp.Value;
            }
        }

        await _bus.Publish(message, rebusHeaders);
    }
}