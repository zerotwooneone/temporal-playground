namespace TemporalDDD.Application.Messaging;

public interface IEventHandler<in TMessage> where TMessage : class
{
    // Now the application developer gets the payload AND the headers
    Task HandleAsync(
        IMessageContext<TMessage> context, 
        CancellationToken cancellationToken);
}