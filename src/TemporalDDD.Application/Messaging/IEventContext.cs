namespace TemporalDDD.Application.Messaging;

public interface IEventContext<out TMessage> where TMessage : IApplicationEvent
{
    TMessage Event { get; }
    string? CorrelationId { get; }
    IReadOnlyDictionary<string, string> Headers { get; }
}