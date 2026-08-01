namespace TemporalDDD.Application.Messaging;

public sealed record EventPublishOptions
{
    public string? CorrelationId { get; init; }
    public IDictionary<string, string>? Headers { get; init; }
    /// <summary>
    /// Overrides the topic name for publishing
    /// </summary>
    public string? OverrideTopic { get; init; }
}