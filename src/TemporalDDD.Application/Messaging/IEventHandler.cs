namespace TemporalDDD.Application.Messaging;

public interface IEventHandler<in TMessage> where TMessage : IApplicationEvent
{
    // Now the application developer gets the payload AND the headers
    Task HandleAsync(
        IEventContext<TMessage> context, 
        CancellationToken cancellationToken);
}