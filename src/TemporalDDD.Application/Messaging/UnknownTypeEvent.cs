namespace TemporalDDD.Application.Messaging;

public sealed record UnknownTypeEvent(string Exception): IApplicationEvent;

public sealed class UnknownTypeEventHandler : IEventHandler<UnknownTypeEvent>
{
    public async Task HandleAsync(IEventContext<UnknownTypeEvent> context, CancellationToken cancellationToken)
    {
        await Console.Error.WriteLineAsync($"Error Unknown Type Event: {context.Event.Exception}");
    }
}