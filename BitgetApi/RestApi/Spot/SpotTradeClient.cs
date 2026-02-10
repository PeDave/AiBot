using System.Text.Json.Serialization;
using BitgetApi.Http;
using BitgetApi.Models;

namespace BitgetApi.RestApi.Spot;

#region Request/Response Models

public class PlaceOrderRequest
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("side")]
    public string Side { get; set; } = string.Empty;

    [JsonPropertyName("orderType")]
    public string OrderType { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public string Size { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public string? Price { get; set; }

    [JsonPropertyName("force")]
    public string? Force { get; set; }

    [JsonPropertyName("clientOid")]
    public string? ClientOrderId { get; set; }
}

public class PlaceOrderResponse
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("clientOid")]
    public string ClientOrderId { get; set; } = string.Empty;
}

public class CancelOrderResponse
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("clientOid")]
    public string ClientOrderId { get; set; } = string.Empty;
}

public class OrderInfo
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("clientOid")]
    public string ClientOrderId { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("side")]
    public string Side { get; set; } = string.Empty;

    [JsonPropertyName("orderType")]
    public string OrderType { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public string Price { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public string Size { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("priceAvg")]
    public string PriceAvg { get; set; } = string.Empty;

    [JsonPropertyName("baseVolume")]
    public string BaseVolume { get; set; } = string.Empty;

    [JsonPropertyName("quoteVolume")]
    public string QuoteVolume { get; set; } = string.Empty;

    [JsonPropertyName("enterPointSource")]
    public string EnterPointSource { get; set; } = string.Empty;

    [JsonPropertyName("cTime")]
    public long CreateTime { get; set; }

    [JsonPropertyName("uTime")]
    public long UpdateTime { get; set; }
}

public class FillInfo
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("tradeId")]
    public string TradeId { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("side")]
    public string Side { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public string Price { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public string Size { get; set; } = string.Empty;

    [JsonPropertyName("feeDetail")]
    public FeeDetail? FeeDetail { get; set; }

    [JsonPropertyName("cTime")]
    public long CreateTime { get; set; }
}

public class FeeDetail
{
    [JsonPropertyName("deduction")]
    public string Deduction { get; set; } = string.Empty;

    [JsonPropertyName("feeCoin")]
    public string FeeCoin { get; set; } = string.Empty;

    [JsonPropertyName("totalDeductionFee")]
    public string TotalDeductionFee { get; set; } = string.Empty;

    [JsonPropertyName("totalFee")]
    public string TotalFee { get; set; } = string.Empty;
}

#endregion

/// <summary>
/// Client for Spot Trade private endpoints
/// </summary>
public class SpotTradeClient
{
    private readonly BitgetHttpClient _httpClient;

    public SpotTradeClient(BitgetHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Place a spot order
    /// </summary>
    public async Task<BitgetResponse<PlaceOrderResponse>> PlaceOrderAsync(
        string symbol,
        string side,
        string orderType,
        string size,
        string? price = null,
        string? force = null,
        string? clientOrderId = null,
        CancellationToken cancellationToken = default)
    {
        var request = new PlaceOrderRequest
        {
            Symbol = symbol,
            Side = side,
            OrderType = orderType,
            Size = size,
            Price = price,
            Force = force,
            ClientOrderId = clientOrderId
        };

        return await _httpClient.PostAsync<PlaceOrderResponse>("/api/v2/spot/trade/place-order", request, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    public async Task<BitgetResponse<CancelOrderResponse>> CancelOrderAsync(string symbol, string orderId, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            symbol,
            orderId
        };

        return await _httpClient.PostAsync<CancelOrderResponse>("/api/v2/spot/trade/cancel-order", request, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Cancel multiple orders
    /// </summary>
    public async Task<BitgetResponse<List<CancelOrderResponse>>> CancelBatchOrdersAsync(List<string> orderIds, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            orderIds
        };

        return await _httpClient.PostAsync<List<CancelOrderResponse>>("/api/v2/spot/trade/cancel-batch-orders", request, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Get open orders
    /// </summary>
    public async Task<BitgetResponse<List<OrderInfo>>> GetOpenOrdersAsync(string? symbol = null, CancellationToken cancellationToken = default)
    {
        var endpoint = "/api/v2/spot/trade/open-orders";
        
        if (!string.IsNullOrWhiteSpace(symbol))
            endpoint += $"?symbol={symbol}";

        return await _httpClient.GetAsync<List<OrderInfo>>(endpoint, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Get order history
    /// </summary>
    public async Task<BitgetResponse<List<OrderInfo>>> GetOrderHistoryAsync(string? symbol = null, int limit = 100, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/api/v2/spot/trade/history-orders?limit={limit}";
        
        if (!string.IsNullOrWhiteSpace(symbol))
            endpoint += $"&symbol={symbol}";

        return await _httpClient.GetAsync<List<OrderInfo>>(endpoint, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Get order details
    /// </summary>
    public async Task<BitgetResponse<OrderInfo>> GetOrderDetailAsync(string orderId, string? clientOrderId = null, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/api/v2/spot/trade/order-info?orderId={orderId}";
        
        if (!string.IsNullOrWhiteSpace(clientOrderId))
            endpoint += $"&clientOid={clientOrderId}";

        return await _httpClient.GetAsync<OrderInfo>(endpoint, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Get fills (trade history)
    /// </summary>
    public async Task<BitgetResponse<List<FillInfo>>> GetFillsAsync(string? symbol = null, string? orderId = null, int limit = 100, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/api/v2/spot/trade/fills?limit={limit}";
        
        if (!string.IsNullOrWhiteSpace(symbol))
            endpoint += $"&symbol={symbol}";
        
        if (!string.IsNullOrWhiteSpace(orderId))
            endpoint += $"&orderId={orderId}";

        return await _httpClient.GetAsync<List<FillInfo>>(endpoint, requiresAuth: true, cancellationToken);
    }
}
