namespace TemporalDDD.Application.Messaging;

public sealed record PublishOptions
{
    public string? CorrelationId { get; init; }
    public IDictionary<string, string>? Headers { get; init; }
    public TimeSpan? Delay { get; init; } // Example: "Don't deliver this for 5 minutes"
    /// <summary>
    /// Overrides the topic name for publishing
    /// </summary>
    public string? OverrideTopic { get; init; }
}