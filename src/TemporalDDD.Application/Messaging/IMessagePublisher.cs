namespace TemporalDDD.Application.Messaging;

public interface IMessagePublisher
{
    Task PublishAsync<TMessage>(
        TMessage message, 
        PublishOptions? options = null,
        CancellationToken cancellationToken = default) where TMessage : class;
}