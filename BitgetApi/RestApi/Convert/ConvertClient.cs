using System.Text.Json.Serialization;
using BitgetApi.Http;
using BitgetApi.Models;

namespace BitgetApi.RestApi.Convert;

#region Response Models

public class ConvertCurrency
{
    [JsonPropertyName("coin")]
    public string Coin { get; set; } = string.Empty;

    [JsonPropertyName("minAmount")]
    public string MinAmount { get; set; } = string.Empty;

    [JsonPropertyName("maxAmount")]
    public string MaxAmount { get; set; } = string.Empty;
}

public class ConvertResponse
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("fromCoin")]
    public string FromCoin { get; set; } = string.Empty;

    [JsonPropertyName("toCoin")]
    public string ToCoin { get; set; } = string.Empty;

    [JsonPropertyName("fromAmount")]
    public string FromAmount { get; set; } = string.Empty;

    [JsonPropertyName("toAmount")]
    public string ToAmount { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public string Price { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public class ConvertRecord
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("fromCoin")]
    public string FromCoin { get; set; } = string.Empty;

    [JsonPropertyName("toCoin")]
    public string ToCoin { get; set; } = string.Empty;

    [JsonPropertyName("fromAmount")]
    public string FromAmount { get; set; } = string.Empty;

    [JsonPropertyName("toAmount")]
    public string ToAmount { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public string Price { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("cTime")]
    public long CreateTime { get; set; }
}

#endregion

/// <summary>
/// Client for Convert API private endpoints
/// </summary>
public class ConvertClient
{
    private readonly BitgetHttpClient _httpClient;

    public ConvertClient(BitgetHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Get available convert currencies
    /// </summary>
    public async Task<BitgetResponse<List<ConvertCurrency>>> GetConvertCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<ConvertCurrency>>("/api/v2/convert/currencies", requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Convert one currency to another
    /// </summary>
    public async Task<BitgetResponse<ConvertResponse>> ConvertAsync(string fromCoin, string toCoin, string amount, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            fromCoin,
            toCoin,
            amount
        };

        return await _httpClient.PostAsync<ConvertResponse>("/api/v2/convert/trade", request, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Get convert history
    /// </summary>
    public async Task<BitgetResponse<List<ConvertRecord>>> GetConvertHistoryAsync(int limit = 100, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<ConvertRecord>>($"/api/v2/convert/convert-record?limit={limit}", requiresAuth: true, cancellationToken);
    }
}
