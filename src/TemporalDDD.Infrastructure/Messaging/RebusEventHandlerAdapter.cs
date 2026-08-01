using Rebus.Handlers;
using Rebus.Pipeline;
using TemporalDDD.Application.Messaging;

namespace TemporalDDD.Infrastructure.Messaging;

public class RebusEventHandlerAdapter<TEvent> : IHandleMessages<TEvent> 
    where TEvent : IApplicationEvent
{
    private readonly IEnumerable<IEventHandler<TEvent>> _handlers;

    public RebusEventHandlerAdapter(IEnumerable<IEventHandler<TEvent>> handlers)
    {
        _handlers = handlers;
    }

    public async Task Handle(TEvent message)
    {
        var messageContext = MessageContext.Current;
        messageContext.Headers.TryGetValue(Rebus.Messages.Headers.CorrelationId, out var correlationId);
        
        var context = new EventContext<TEvent>
        {
            Event = message,
            CorrelationId = correlationId,
            Headers = messageContext.Headers
        };
        foreach (var handler in _handlers)
        {
            await handler.HandleAsync(context, CancellationToken.None);
        }
    }
}

internal sealed record EventContext<T>: IEventContext<T> where T : IApplicationEvent
{
    public T Event { get; init; }
    public string? CorrelationId { get; init; }
    public IReadOnlyDictionary<string, string> Headers { get; init; }
}