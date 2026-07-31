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

    public async Task PublishAsync<TMessage>(
        TMessage message, 
        PublishOptions? options = null, 
        CancellationToken cancellationToken = default) where TMessage : class
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

        if (options?.Delay.HasValue == true)
        {
            await _bus.Defer(options.Delay.Value, message, rebusHeaders);
        }
        else
        {
            await _bus.Publish(message, rebusHeaders);
        }
    }
}