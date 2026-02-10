using System.Text.Json.Serialization;
using BitgetApi.Http;
using BitgetApi.Models;

namespace BitgetApi.RestApi.Common;

public class ServerTimeResponse
{
    [JsonPropertyName("serverTime")]
    public long ServerTime { get; set; }
}

public class AnnouncementResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("publishTime")]
    public long PublishTime { get; set; }
}

/// <summary>
/// Client for Common API endpoints
/// </summary>
public class CommonApiClient
{
    private readonly BitgetHttpClient _httpClient;

    public CommonApiClient(BitgetHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Get server time
    /// </summary>
    public async Task<BitgetResponse<ServerTimeResponse>> GetServerTimeAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<ServerTimeResponse>("/api/v2/public/time", requiresAuth: false, cancellationToken);
    }

    /// <summary>
    /// Get announcements
    /// </summary>
    public async Task<BitgetResponse<List<AnnouncementResponse>>> GetAnnouncementsAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<AnnouncementResponse>>("/api/v2/public/announcements", requiresAuth: false, cancellationToken);
    }
}
