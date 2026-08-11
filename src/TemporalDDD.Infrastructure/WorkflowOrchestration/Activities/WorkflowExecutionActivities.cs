using System.Text.Json;
using Temporalio.Activities;
using TemporalDDD.Infrastructure.Testing;

namespace TemporalDDD.Infrastructure.WorkflowOrchestration.Activities;

public class WorkflowExecutionActivities : IWorkflowExecutionActivities
{
    private readonly ChaosHttpClient _chaosHttpClient;

    public WorkflowExecutionActivities(ChaosHttpClient chaosHttpClient)
    {
        _chaosHttpClient = chaosHttpClient;
    }

    [Activity]
    public async Task<JsonDocument> ExecuteApiCallAsync(ExecuteApiInput input)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(input.EndpointUrl))
        {
            throw new InvalidOperationException($"Internal Corruption: EndpointUrl is required for node {input.NodeId}");
        }

        // Simulate external API call with chaos (100ms latency, 10% failure rate)
        var response = await _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError)
            .GetAsync(input.EndpointUrl);

        response.EnsureSuccessStatusCode();

        // Read response content
        var content = await response.Content.ReadAsStringAsync();
        
        // Return empty JSON document if content is empty or invalid
        if (string.IsNullOrWhiteSpace(content))
        {
            return JsonDocument.Parse("{}");
        }

        try
        {
            return JsonDocument.Parse(content);
        }
        catch (JsonException)
        {
            // If response is not valid JSON, return empty document
            return JsonDocument.Parse("{}");
        }
    }

    [Activity]
    public async Task SendNotificationAsync(SendNotificationInput input)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(input.MessageTemplate))
        {
            throw new InvalidOperationException($"Internal Corruption: MessageTemplate is required for node {input.NodeId}");
        }

        // Simulate external notification webhook call with chaos (100ms latency, 10% failure rate)
        var response = await _chaosHttpClient
            .WithLatency(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100))
            .WithFailureRate(0.10, System.Net.HttpStatusCode.InternalServerError)
            .PostAsJsonAsync("https://notifications.example.com/api/send", new { Message = input.MessageTemplate });

        response.EnsureSuccessStatusCode();

        Console.WriteLine($"[WorkflowNotification] Notification sent for node {input.NodeId}: {input.MessageTemplate}");
    }
}
