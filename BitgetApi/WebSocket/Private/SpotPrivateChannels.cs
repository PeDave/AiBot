using System.Text.Json;
using System.Text.Json.Serialization;

namespace BitgetApi.WebSocket.Private;

#region Data Models

public class OrderUpdateData
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

    [JsonPropertyName("fillPx")]
    public string FillPrice { get; set; } = string.Empty;

    [JsonPropertyName("fillSz")]
    public string FillSize { get; set; } = string.Empty;

    [JsonPropertyName("accFillSz")]
    public string AccumulatedFillSize { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("cTime")]
    public long CreateTime { get; set; }

    [JsonPropertyName("uTime")]
    public long UpdateTime { get; set; }
}

public class AccountUpdateData
{
    [JsonPropertyName("coin")]
    public string Coin { get; set; } = string.Empty;

    [JsonPropertyName("available")]
    public string Available { get; set; } = string.Empty;

    [JsonPropertyName("frozen")]
    public string Frozen { get; set; } = string.Empty;

    [JsonPropertyName("locked")]
    public string Locked { get; set; } = string.Empty;

    [JsonPropertyName("uTime")]
    public long UpdateTime { get; set; }
}

#endregion

/// <summary>
/// Spot private WebSocket channels
/// </summary>
public class SpotPrivateChannels
{
    private readonly BitgetWebSocketClient _webSocket;

    public SpotPrivateChannels(BitgetWebSocketClient webSocket)
    {
        _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
    }

    /// <summary>
    /// Subscribe to order updates
    /// </summary>
    public async Task SubscribeOrdersAsync(Action<OrderUpdateData> callback, CancellationToken cancellationToken = default)
    {
        var channel = "orders";
        await _webSocket.SubscribeAsync(channel, instType: "sp", isPrivate: true, cancellationToken: cancellationToken);

        _webSocket.AddSubscription($"{channel}_spot", message =>
        {
            try
            {
                var response = JsonSerializer.Deserialize<WebSocketResponse<OrderUpdateData>>(message);
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
    /// Subscribe to account updates
    /// </summary>
    public async Task SubscribeAccountAsync(Action<AccountUpdateData> callback, CancellationToken cancellationToken = default)
    {
        var channel = "account";
        await _webSocket.SubscribeAsync(channel, instType: "sp", isPrivate: true, cancellationToken: cancellationToken);

        _webSocket.AddSubscription($"{channel}_spot", message =>
        {
            try
            {
                var response = JsonSerializer.Deserialize<WebSocketResponse<AccountUpdateData>>(message);
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
