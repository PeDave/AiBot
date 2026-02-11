using System.Text.Json.Serialization;
using BitgetApi.Http;
using BitgetApi.Models;

namespace BitgetApi.RestApi.Futures;

#region Response Models

public class FuturesOrderResponse
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("clientOid")]
    public string ClientOrderId { get; set; } = string.Empty;
}

public class FuturesOrderInfo
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public string Size { get; set; } = string.Empty;

    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("clientOid")]
    public string ClientOrderId { get; set; } = string.Empty;

    [JsonPropertyName("basePrice")]
    public string BasePrice { get; set; } = string.Empty;

    [JsonPropertyName("enterPointSource")]
    public string EnterPointSource { get; set; } = string.Empty;

    [JsonPropertyName("orderType")]
    public string OrderType { get; set; } = string.Empty;

    [JsonPropertyName("side")]
    public string Side { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("priceAvg")]
    public string PriceAvg { get; set; } = string.Empty;

    [JsonPropertyName("baseVolume")]
    public string BaseVolume { get; set; } = string.Empty;

    [JsonPropertyName("leverage")]
    public string Leverage { get; set; } = string.Empty;

    [JsonPropertyName("marginCoin")]
    public string MarginCoin { get; set; } = string.Empty;

    [JsonPropertyName("marginMode")]
    public string MarginMode { get; set; } = string.Empty;

    [JsonPropertyName("fee")]
    public string Fee { get; set; } = string.Empty;

    [JsonPropertyName("cTime")]
    public long CreateTime { get; set; }

    [JsonPropertyName("uTime")]
    public long UpdateTime { get; set; }
}

public class PlanOrderResponse
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("clientOid")]
    public string ClientOrderId { get; set; } = string.Empty;
}

#endregion

/// <summary>
/// Client for Futures Trade private endpoints
/// </summary>
public class FuturesTradeClient
{
    private readonly BitgetHttpClient _httpClient;

    public FuturesTradeClient(BitgetHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Place a futures order
    /// </summary>
    public async Task<BitgetResponse<FuturesOrderResponse>> PlaceOrderAsync(
        string symbol,
        string marginCoin,
        string side,
        string orderType,
        string size,
        string? price = null,
        string? clientOrderId = null,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            symbol,
            marginCoin,
            side,
            orderType,
            size,
            price,
            clientOid = clientOrderId
        };

        return await _httpClient.PostAsync<FuturesOrderResponse>("/api/v2/mix/order/place-order", request, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Cancel a futures order
    /// </summary>
    public async Task<BitgetResponse<FuturesOrderResponse>> CancelOrderAsync(string symbol, string marginCoin, string orderId, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            symbol,
            marginCoin,
            orderId
        };

        return await _httpClient.PostAsync<FuturesOrderResponse>("/api/v2/mix/order/cancel-order", request, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Get open orders
    /// </summary>
    public async Task<BitgetResponse<List<FuturesOrderInfo>>> GetOpenOrdersAsync(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty", nameof(symbol));

        return await _httpClient.GetAsync<List<FuturesOrderInfo>>($"/api/v2/mix/order/open-orders?symbol={symbol}", requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Get history orders
    /// </summary>
    public async Task<BitgetResponse<List<FuturesOrderInfo>>> GetHistoryOrdersAsync(string symbol, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty", nameof(symbol));

        return await _httpClient.GetAsync<List<FuturesOrderInfo>>($"/api/v2/mix/order/history-orders?symbol={symbol}&pageSize={pageSize}", requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Place a plan order (conditional order)
    /// </summary>
    public async Task<BitgetResponse<PlanOrderResponse>> PlacePlanOrderAsync(
        string symbol,
        string marginCoin,
        string side,
        string triggerPrice,
        string executePrice,
        string size,
        string? clientOrderId = null,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            symbol,
            marginCoin,
            side,
            triggerPrice,
            executePrice,
            size,
            clientOid = clientOrderId
        };

        return await _httpClient.PostAsync<PlanOrderResponse>("/api/v2/mix/order/place-plan-order", request, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Cancel a plan order
    /// </summary>
    public async Task<BitgetResponse<PlanOrderResponse>> CancelPlanOrderAsync(string symbol, string marginCoin, string planOrderId, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            symbol,
            marginCoin,
            orderId = planOrderId
        };

        return await _httpClient.PostAsync<PlanOrderResponse>("/api/v2/mix/order/cancel-plan-order", request, requiresAuth: true, cancellationToken);
    }
}
