using System.Text.Json;
using System.Text.Json.Serialization;

namespace BitgetApi.WebSocket.Private;

#region Data Models

public class FuturesOrderUpdateData
{
    [JsonPropertyName("instId")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("ordId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("clOrdId")]
    public string ClientOrderId { get; set; } = string.Empty;

    [JsonPropertyName("px")]
    public string Price { get; set; } = string.Empty;

    [JsonPropertyName("sz")]
    public string Size { get; set; } = string.Empty;

    [JsonPropertyName("ordType")]
    public string OrderType { get; set; } = string.Empty;

    [JsonPropertyName("side")]
    public string Side { get; set; } = string.Empty;

    [JsonPropertyName("posSide")]
    public string PositionSide { get; set; } = string.Empty;

    [JsonPropertyName("fillPx")]
    public string FillPrice { get; set; } = string.Empty;

    [JsonPropertyName("fillSz")]
    public string FillSize { get; set; } = string.Empty;

    [JsonPropertyName("accFillSz")]
    public string AccumulatedFillSize { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("lever")]
    public string Leverage { get; set; } = string.Empty;

    [JsonPropertyName("cTime")]
    public long CreateTime { get; set; }

    [JsonPropertyName("uTime")]
    public long UpdateTime { get; set; }
}

public class PositionUpdateData
{
    [JsonPropertyName("instId")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("posId")]
    public string PositionId { get; set; } = string.Empty;

    [JsonPropertyName("marginCoin")]
    public string MarginCoin { get; set; } = string.Empty;

    [JsonPropertyName("marginMode")]
    public string MarginMode { get; set; } = string.Empty;

    [JsonPropertyName("holdSide")]
    public string HoldSide { get; set; } = string.Empty;

    [JsonPropertyName("holdMode")]
    public string HoldMode { get; set; } = string.Empty;

    [JsonPropertyName("total")]
    public string Total { get; set; } = string.Empty;

    [JsonPropertyName("available")]
    public string Available { get; set; } = string.Empty;

    [JsonPropertyName("frozen")]
    public string Frozen { get; set; } = string.Empty;

    [JsonPropertyName("openPriceAvg")]
    public string OpenPriceAvg { get; set; } = string.Empty;

    [JsonPropertyName("leverage")]
    public string Leverage { get; set; } = string.Empty;

    [JsonPropertyName("achievedProfits")]
    public string AchievedProfits { get; set; } = string.Empty;

    [JsonPropertyName("upl")]
    public string UnrealizedPL { get; set; } = string.Empty;

    [JsonPropertyName("uplRatio")]
    public string UnrealizedPLRatio { get; set; } = string.Empty;

    [JsonPropertyName("liqPx")]
    public string LiquidationPrice { get; set; } = string.Empty;

    [JsonPropertyName("markPx")]
    public string MarkPrice { get; set; } = string.Empty;

    [JsonPropertyName("cTime")]
    public long CreateTime { get; set; }

    [JsonPropertyName("uTime")]
    public long UpdateTime { get; set; }
}

public class FuturesAccountUpdateData
{
    [JsonPropertyName("marginCoin")]
    public string MarginCoin { get; set; } = string.Empty;

    [JsonPropertyName("locked")]
    public string Locked { get; set; } = string.Empty;

    [JsonPropertyName("available")]
    public string Available { get; set; } = string.Empty;

    [JsonPropertyName("maxOpenPosAvailable")]
    public string MaxOpenPosAvailable { get; set; } = string.Empty;

    [JsonPropertyName("maxTransferOut")]
    public string MaxTransferOut { get; set; } = string.Empty;

    [JsonPropertyName("equity")]
    public string Equity { get; set; } = string.Empty;

    [JsonPropertyName("usdtEquity")]
    public string UsdtEquity { get; set; } = string.Empty;

    [JsonPropertyName("uTime")]
    public long UpdateTime { get; set; }
}

#endregion

/// <summary>
/// Futures private WebSocket channels
/// </summary>
public class FuturesPrivateChannels
{
    private readonly BitgetWebSocketClient _webSocket;

    public FuturesPrivateChannels(BitgetWebSocketClient webSocket)
    {
        _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
    }

    /// <summary>
    /// Subscribe to futures order updates
    /// </summary>
    public async Task SubscribeOrdersAsync(Action<FuturesOrderUpdateData> callback, CancellationToken cancellationToken = default)
    {
        var channel = "orders";
        await _webSocket.SubscribeAsync(channel, instType: "mc", isPrivate: true, cancellationToken: cancellationToken);

        _webSocket.AddSubscription($"{channel}_futures", message =>
        {
            try
            {
                var response = JsonSerializer.Deserialize<WebSocketResponse<FuturesOrderUpdateData>>(message);
                if (response?.Data != null)
                {
                    foreach (var data in response.Data)
                    {
                        callback(data);
                    }
                }
            }
            catch { /* Ignore parse errors */ }
        });
    }

    /// <summary>
    /// Subscribe to position updates
    /// </summary>
    public async Task SubscribePositionsAsync(Action<PositionUpdateData> callback, CancellationToken cancellationToken = default)
    {
        var channel = "positions";
        await _webSocket.SubscribeAsync(channel, instType: "mc", isPrivate: true, cancellationToken: cancellationToken);

        _webSocket.AddSubscription($"{channel}_futures", message =>
        {
            try
            {
                var response = JsonSerializer.Deserialize<WebSocketResponse<PositionUpdateData>>(message);
                if (response?.Data != null)
                {
                    foreach (var data in response.Data)
                    {
                        callback(data);
                    }
                }
            }
            catch { /* Ignore parse errors */ }
        });
    }

    /// <summary>
    /// Subscribe to futures account updates
    /// </summary>
    public async Task SubscribeAccountAsync(Action<FuturesAccountUpdateData> callback, CancellationToken cancellationToken = default)
    {
        var channel = "account";
        await _webSocket.SubscribeAsync(channel, instType: "mc", isPrivate: true, cancellationToken: cancellationToken);

        _webSocket.AddSubscription($"{channel}_futures", message =>
        {
            try
            {
                var response = JsonSerializer.Deserialize<WebSocketResponse<FuturesAccountUpdateData>>(message);
                if (response?.Data != null)
                {
                    foreach (var data in response.Data)
                    {
                        callback(data);
                    }
                }
            }
            catch { /* Ignore parse errors */ }
        });
    }
}
