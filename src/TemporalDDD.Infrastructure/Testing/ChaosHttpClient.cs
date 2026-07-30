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
///     .WithFailureRate(0.10, HttpStatusCode.GatewayTimeout)
///     .ReturnsJson(new { Status = "Cleared", Provider = providerId });
/// 
/// var response = await client.GetAsync($"/api/background-checks/{providerId}");
/// </code>
/// </summary>
public class ChaosHttpClient
{
    private readonly Random _random;
    
    private readonly TimeSpan _minLatency;
    private readonly TimeSpan _maxLatency;
    private readonly IReadOnlyDictionary<HttpStatusCode, double> _failureScenarios;
    private readonly double _networkDropRate;
    private readonly string? _defaultJsonResponse;
    private readonly string _defaultContentType;

    public ChaosHttpClient(Random random)
    {
        _random = random;
        _minLatency = TimeSpan.Zero;
        _maxLatency = TimeSpan.Zero;
        _failureScenarios = new Dictionary<HttpStatusCode, double>();
        _networkDropRate = 0.0;
        _defaultJsonResponse = null;
        _defaultContentType = "application/json";
    }

    private ChaosHttpClient(
        Random random,
        TimeSpan minLatency,
        TimeSpan maxLatency,
        IReadOnlyDictionary<HttpStatusCode, double> failureScenarios,
        double networkDropRate,
        string? defaultJsonResponse,
        string defaultContentType)
    {
        _random = random;
        _minLatency = minLatency;
        _maxLatency = maxLatency;
        _failureScenarios = failureScenarios;
        _networkDropRate = networkDropRate;
        _defaultJsonResponse = defaultJsonResponse;
        _defaultContentType = defaultContentType;
    }

    public ChaosHttpClient WithLatency(TimeSpan min, TimeSpan max)
    {
        if (min < TimeSpan.Zero)
            throw new ArgumentException("Minimum latency cannot be negative", nameof(min));
        if (max < min)
            throw new ArgumentException("Maximum latency must be greater than or equal to minimum latency", nameof(max));

        return new ChaosHttpClient(
            random: _random,
            minLatency: min,
            maxLatency: max,
            failureScenarios: new Dictionary<HttpStatusCode, double>(_failureScenarios),
            networkDropRate: _networkDropRate,
            defaultJsonResponse: _defaultJsonResponse,
            defaultContentType: _defaultContentType
        );
    }

    public ChaosHttpClient WithFailureRate(double probability, HttpStatusCode statusCode)
    {
        if (probability < 0.0 || probability > 1.0)
            throw new ArgumentException("Probability must be between 0.0 and 1.0", nameof(probability));

        var newScenarios = new Dictionary<HttpStatusCode, double>(_failureScenarios)
        {
            [statusCode] = probability
        };

        return new ChaosHttpClient(
            random: _random,
            minLatency: _minLatency,
            maxLatency: _maxLatency,
            failureScenarios: newScenarios,
            networkDropRate: _networkDropRate,
            defaultJsonResponse: _defaultJsonResponse,
            defaultContentType: _defaultContentType
        );
    }

    public ChaosHttpClient WithNetworkDropRate(double probability)
    {
        if (probability < 0.0 || probability > 1.0)
            throw new ArgumentException("Probability must be between 0.0 and 1.0", nameof(probability));

        return new ChaosHttpClient(
            random: _random,
            minLatency: _minLatency,
            maxLatency: _maxLatency,
            failureScenarios: new Dictionary<HttpStatusCode, double>(_failureScenarios),
            networkDropRate: probability,
            defaultJsonResponse: _defaultJsonResponse,
            defaultContentType: _defaultContentType
        );
    }

    public ChaosHttpClient ReturnsJson<T>(T payload)
    {
        return new ChaosHttpClient(
            random: _random,
            minLatency: _minLatency,
            maxLatency: _maxLatency,
            failureScenarios: new Dictionary<HttpStatusCode, double>(_failureScenarios),
            networkDropRate: _networkDropRate,
            defaultJsonResponse: JsonSerializer.Serialize(payload),
            defaultContentType: _defaultContentType
        );
    }

    public ChaosHttpClient ReturnsJson(string json)
    {
        return new ChaosHttpClient(
            random: _random,
            minLatency: _minLatency,
            maxLatency: _maxLatency,
            failureScenarios: new Dictionary<HttpStatusCode, double>(_failureScenarios),
            networkDropRate: _networkDropRate,
            defaultJsonResponse: json,
            defaultContentType: _defaultContentType
        );
    }

    public ChaosHttpClient WithContentType(string contentType)
    {
        return new ChaosHttpClient(
            random: _random,
            minLatency: _minLatency,
            maxLatency: _maxLatency,
            failureScenarios: new Dictionary<HttpStatusCode, double>(_failureScenarios),
            networkDropRate: _networkDropRate,
            defaultJsonResponse: _defaultJsonResponse,
            defaultContentType: contentType
        );
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
        var delay = GetRandomDelay();
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, cancellationToken);
        }

        // Check for network drop (hard failure)
        if (_networkDropRate > 0.0 && _random.NextDouble() < _networkDropRate)
        {
            throw new HttpRequestException("Chaos simulation: Simulated network drop");
        }

        // Check for failure injection (graceful HTTP error)
        foreach (var (statusCode, probability) in _failureScenarios)
        {
            if (_random.NextDouble() < probability)
            {
                return new HttpResponseMessage(statusCode)
                {
                    ReasonPhrase = "Chaos simulation: Simulated failure"
                };
            }
        }

        // Return successful response
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(_defaultJsonResponse ?? "{}", Encoding.UTF8, _defaultContentType)
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
