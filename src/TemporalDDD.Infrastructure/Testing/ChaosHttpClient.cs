using System.Net;
using System.Text;
using System.Text.Json;

namespace TemporalDDD.Infrastructure.Testing;

/// <summary>
/// Standalone HTTP client with chaos simulation for testing Temporal workflows.
/// Simulates latency, random failures, network drops, and returns fake responses.
/// 
/// Usage example:
/// <code>
/// var client = new ChaosHttpClient()
///     .WithLatency(TimeSpan.FromMilliseconds(200), TimeSpan.FromSeconds(2))
///     .WithFailureRate(0.40, HttpStatusCode.ServiceUnavailable)
///     .ReturnsJson(new { Status = "Cleared", Provider = providerId });
/// 
/// var response = await client.GetAsync($"/api/background-checks/{providerId}");
/// </code>
/// </summary>
public class ChaosHttpClient
{
    private readonly Random _random = new();
    private readonly object _lock = new();
    
    private TimeSpan _minLatency = TimeSpan.Zero;
    private TimeSpan _maxLatency = TimeSpan.Zero;
    private double _failureRate = 0.0;
    private HttpStatusCode _failureStatusCode = HttpStatusCode.InternalServerError;
    private double _networkDropRate = 0.0;
    private string? _defaultJsonResponse;
    private string _defaultContentType = "application/json";

    public ChaosHttpClient WithLatency(TimeSpan min, TimeSpan max)
    {
        if (min < TimeSpan.Zero)
            throw new ArgumentException("Minimum latency cannot be negative", nameof(min));
        if (max < min)
            throw new ArgumentException("Maximum latency must be greater than or equal to minimum latency", nameof(max));

        lock (_lock)
        {
            _minLatency = min;
            _maxLatency = max;
        }
        return this;
    }

    public ChaosHttpClient WithFailureRate(double probability, HttpStatusCode statusCode)
    {
        if (probability < 0.0 || probability > 1.0)
            throw new ArgumentException("Probability must be between 0.0 and 1.0", nameof(probability));

        lock (_lock)
        {
            _failureRate = probability;
            _failureStatusCode = statusCode;
        }
        return this;
    }

    public ChaosHttpClient WithNetworkDropRate(double probability)
    {
        if (probability < 0.0 || probability > 1.0)
            throw new ArgumentException("Probability must be between 0.0 and 1.0", nameof(probability));

        lock (_lock)
        {
            _networkDropRate = probability;
        }
        return this;
    }

    public ChaosHttpClient ReturnsJson<T>(T payload)
    {
        lock (_lock)
        {
            _defaultJsonResponse = JsonSerializer.Serialize(payload);
        }
        return this;
    }

    public ChaosHttpClient ReturnsJson(string json)
    {
        lock (_lock)
        {
            _defaultJsonResponse = json;
        }
        return this;
    }

    public ChaosHttpClient WithContentType(string contentType)
    {
        lock (_lock)
        {
            _defaultContentType = contentType;
        }
        return this;
    }

    public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        return await SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri), cancellationToken);
    }

    public async Task<HttpResponseMessage> PostAsJsonAsync<TValue>(string requestUri, TValue value, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json")
        };
        return await SendAsync(request, cancellationToken);
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        // Apply latency if configured
        TimeSpan delay;
        lock (_lock)
        {
            delay = GetRandomDelay();
        }

        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, cancellationToken);
        }

        // Check for network drop (hard failure)
        double dropRate;
        lock (_lock)
        {
            dropRate = _networkDropRate;
        }

        if (dropRate > 0.0 && _random.NextDouble() < dropRate)
        {
            throw new HttpRequestException("Chaos simulation: Simulated network drop");
        }

        // Check for failure injection (graceful HTTP error)
        double failureRate;
        HttpStatusCode failureStatusCode;
        lock (_lock)
        {
            failureRate = _failureRate;
            failureStatusCode = _failureStatusCode;
        }

        if (failureRate > 0.0 && _random.NextDouble() < failureRate)
        {
            return new HttpResponseMessage(failureStatusCode)
            {
                ReasonPhrase = "Chaos simulation: Simulated failure"
            };
        }

        // Return successful response
        string? jsonResponse;
        string contentType;
        lock (_lock)
        {
            jsonResponse = _defaultJsonResponse;
            contentType = _defaultContentType;
        }

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(jsonResponse ?? "{}", Encoding.UTF8, contentType)
        };

        return response;
    }

    private TimeSpan GetRandomDelay()
    {
        if (_minLatency == _maxLatency)
            return _minLatency;

        var range = _maxLatency - _minLatency;
        var randomMilliseconds = _random.NextDouble() * range.TotalMilliseconds;
        return _minLatency + TimeSpan.FromMilliseconds(randomMilliseconds);
    }
}
