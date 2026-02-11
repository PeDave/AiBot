using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BitgetApi.Auth;
using BitgetApi.Models;
using Microsoft.Extensions.Logging;

namespace BitgetApi.Http;

/// <summary>
/// HTTP client for Bitget API communication
/// </summary>
public class BitgetHttpClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly BitgetAuthenticator? _authenticator;
    private readonly ILogger<BitgetHttpClient>? _logger;
    private readonly SemaphoreSlim _rateLimitSemaphore;
    private readonly TimeSpan _rateLimitDelay = TimeSpan.FromMilliseconds(100); // 10 requests per second max

    public const string BaseUrl = "https://api.bitget.com";

    public BitgetHttpClient(BitgetAuthenticator? authenticator = null, ILogger<BitgetHttpClient>? logger = null)
    {
        _authenticator = authenticator;
        _logger = logger;
        _rateLimitSemaphore = new SemaphoreSlim(1, 1);

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };

        _httpClient.DefaultRequestHeaders.Add("User-Agent", "BitgetApi-CSharp/1.0");
    }

    /// <summary>
    /// Sends a GET request to the specified endpoint
    /// </summary>
    public async Task<BitgetResponse<T>> GetAsync<T>(string endpoint, bool requiresAuth = false, CancellationToken cancellationToken = default)
    {
        await ApplyRateLimitAsync(cancellationToken);

        var headers = new Dictionary<string, string>();
        
        if (requiresAuth)
        {
            if (_authenticator == null)
                throw new InvalidOperationException("Authentication required but no credentials provided");
            
            _authenticator.AddAuthHeaders(headers, "GET", endpoint);
        }

        return await SendRequestAsync<T>(HttpMethod.Get, endpoint, null, headers, cancellationToken);
    }

    /// <summary>
    /// Sends a POST request to the specified endpoint
    /// </summary>
    public async Task<BitgetResponse<T>> PostAsync<T>(string endpoint, object? body = null, bool requiresAuth = true, CancellationToken cancellationToken = default)
    {
        await ApplyRateLimitAsync(cancellationToken);

        var bodyJson = body != null ? JsonSerializer.Serialize(body) : "";
        var headers = new Dictionary<string, string>();

        if (requiresAuth)
        {
            if (_authenticator == null)
                throw new InvalidOperationException("Authentication required but no credentials provided");
            
            _authenticator.AddAuthHeaders(headers, "POST", endpoint, bodyJson);
        }

        return await SendRequestAsync<T>(HttpMethod.Post, endpoint, bodyJson, headers, cancellationToken);
    }

    /// <summary>
    /// Sends a DELETE request to the specified endpoint
    /// </summary>
    public async Task<BitgetResponse<T>> DeleteAsync<T>(string endpoint, object? body = null, bool requiresAuth = true, CancellationToken cancellationToken = default)
    {
        await ApplyRateLimitAsync(cancellationToken);

        var bodyJson = body != null ? JsonSerializer.Serialize(body) : "";
        var headers = new Dictionary<string, string>();

        if (requiresAuth)
        {
            if (_authenticator == null)
                throw new InvalidOperationException("Authentication required but no credentials provided");
            
            _authenticator.AddAuthHeaders(headers, "DELETE", endpoint, bodyJson);
        }

        return await SendRequestAsync<T>(HttpMethod.Delete, endpoint, bodyJson, headers, cancellationToken);
    }

    private async Task<BitgetResponse<T>> SendRequestAsync<T>(
        HttpMethod method,
        string endpoint,
        string? body,
        Dictionary<string, string> headers,
        CancellationToken cancellationToken)
    {
        var maxRetries = 3;
        var retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                using var request = new HttpRequestMessage(method, endpoint);

                foreach (var header in headers)
                {
                    request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                if (!string.IsNullOrEmpty(body) && (method == HttpMethod.Post || method == HttpMethod.Delete))
                {
                    request.Content = new StringContent(body, Encoding.UTF8, "application/json");
                }

                _logger?.LogDebug("Sending {Method} request to {Endpoint}", method, endpoint);

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger?.LogDebug("Received response: {StatusCode} - {Content}", response.StatusCode, responseContent);

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    retryCount++;
                    if (retryCount < maxRetries)
                    {
                        var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                        _logger?.LogWarning("Rate limited, retrying after {Delay}s (attempt {Retry}/{Max})", delay.TotalSeconds, retryCount, maxRetries);
                        await Task.Delay(delay, cancellationToken);
                        continue;
                    }
                }

                if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.BadRequest)
                {
                    throw new HttpRequestException($"HTTP {response.StatusCode}: {responseContent}");
                }

                var result = JsonSerializer.Deserialize<BitgetResponse<T>>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result ?? throw new InvalidOperationException("Failed to deserialize response");
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                retryCount++;
                if (retryCount >= maxRetries)
                    throw new TimeoutException($"Request to {endpoint} timed out after {maxRetries} attempts");
                
                _logger?.LogWarning("Request timeout, retrying (attempt {Retry}/{Max})", retryCount, maxRetries);
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        throw new InvalidOperationException("Max retries exceeded");
    }

    private async Task ApplyRateLimitAsync(CancellationToken cancellationToken)
    {
        await _rateLimitSemaphore.WaitAsync(cancellationToken);
        try
        {
            await Task.Delay(_rateLimitDelay, cancellationToken);
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _rateLimitSemaphore?.Dispose();
        GC.SuppressFinalize(this);
    }
}
