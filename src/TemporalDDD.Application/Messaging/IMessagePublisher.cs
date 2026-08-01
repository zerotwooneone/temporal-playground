namespace TemporalDDD.Application.Messaging;

public interface IMessagePublisher
{
    Task PublishEventAsync(
        IApplicationEvent message, 
        EventPublishOptions? options = null,
        CancellationToken cancellationToken = default);
}