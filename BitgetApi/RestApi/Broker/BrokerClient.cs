using System.Text.Json.Serialization;
using BitgetApi.Http;
using BitgetApi.Models;

namespace BitgetApi.RestApi.Broker;

#region Response Models

public class SubAccountInfo
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("cTime")]
    public long CreateTime { get; set; }
}

public class CreateSubAccountResponse
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public class SubAccountApiKey
{
    [JsonPropertyName("apiKey")]
    public string ApiKey { get; set; } = string.Empty;

    [JsonPropertyName("secretKey")]
    public string SecretKey { get; set; } = string.Empty;

    [JsonPropertyName("passphrase")]
    public string Passphrase { get; set; } = string.Empty;

    [JsonPropertyName("permissions")]
    public List<string> Permissions { get; set; } = new();

    [JsonPropertyName("ipWhiteList")]
    public List<string> IpWhiteList { get; set; } = new();

    [JsonPropertyName("cTime")]
    public long CreateTime { get; set; }
}

#endregion

/// <summary>
/// Client for Broker API private endpoints
/// </summary>
public class BrokerClient
{
    private readonly BitgetHttpClient _httpClient;

    public BrokerClient(BitgetHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Get sub-accounts list
    /// </summary>
    public async Task<BitgetResponse<List<SubAccountInfo>>> GetSubAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<SubAccountInfo>>("/api/v2/broker/account/sub-list", requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Create a sub-account
    /// </summary>
    public async Task<BitgetResponse<CreateSubAccountResponse>> CreateSubAccountAsync(string subAccountName, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            label = subAccountName
        };

        return await _httpClient.PostAsync<CreateSubAccountResponse>("/api/v2/broker/account/create-sub", request, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Get sub-account API keys
    /// </summary>
    public async Task<BitgetResponse<List<SubAccountApiKey>>> GetSubAccountApiKeyAsync(string subUid, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subUid))
            throw new ArgumentException("Sub UID cannot be empty", nameof(subUid));

        return await _httpClient.GetAsync<List<SubAccountApiKey>>($"/api/v2/broker/account/sub-api-list?subUid={subUid}", requiresAuth: true, cancellationToken);
    }
}
