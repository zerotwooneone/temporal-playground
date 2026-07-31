namespace TemporalDDD.Application.Messaging;

public interface IMessageContext<out TMessage> where TMessage : class
{
    TMessage Message { get; }
    string MessageId { get; }
    string? CorrelationId { get; }
    IReadOnlyDictionary<string, string> Headers { get; }
}