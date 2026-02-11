using System.Text.Json.Serialization;
using BitgetApi.Http;
using BitgetApi.Models;

namespace BitgetApi.RestApi.Tax;

#region Response Models

public class SpotTransactionRecord
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("tradeId")]
    public string TradeId { get; set; } = string.Empty;

    [JsonPropertyName("side")]
    public string Side { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public string Price { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public string Size { get; set; } = string.Empty;

    [JsonPropertyName("fee")]
    public string Fee { get; set; } = string.Empty;

    [JsonPropertyName("feeCoin")]
    public string FeeCoin { get; set; } = string.Empty;

    [JsonPropertyName("cTime")]
    public long CreateTime { get; set; }
}

public class FuturesTransactionRecord
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("tradeId")]
    public string TradeId { get; set; } = string.Empty;

    [JsonPropertyName("side")]
    public string Side { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public string Price { get; set; } = string.Empty;

    [JsonPropertyName("baseVolume")]
    public string BaseVolume { get; set; } = string.Empty;

    [JsonPropertyName("fee")]
    public string Fee { get; set; } = string.Empty;

    [JsonPropertyName("feeCoin")]
    public string FeeCoin { get; set; } = string.Empty;

    [JsonPropertyName("profit")]
    public string Profit { get; set; } = string.Empty;

    [JsonPropertyName("cTime")]
    public long CreateTime { get; set; }
}

#endregion

/// <summary>
/// Client for Tax reporting endpoints
/// </summary>
public class TaxClient
{
    private readonly BitgetHttpClient _httpClient;

    public TaxClient(BitgetHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Get spot transaction history for tax reporting
    /// </summary>
    public async Task<BitgetResponse<List<SpotTransactionRecord>>> GetSpotTransactionHistoryAsync(long startTime, long endTime, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<SpotTransactionRecord>>($"/api/v2/tax/spot-record?startTime={startTime}&endTime={endTime}", requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Get futures transaction history for tax reporting
    /// </summary>
    public async Task<BitgetResponse<List<FuturesTransactionRecord>>> GetFuturesTransactionHistoryAsync(long startTime, long endTime, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<FuturesTransactionRecord>>($"/api/v2/tax/future-record?startTime={startTime}&endTime={endTime}", requiresAuth: true, cancellationToken);
    }
}
