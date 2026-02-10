using System.Text.Json;
using System.Text.Json.Serialization;

namespace BitgetApi.WebSocket.Public;

#region Data Models

public class FuturesTickerData
{
    [JsonPropertyName("instId")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("lastPr")]
    public string LastPrice { get; set; } = string.Empty;

    [JsonPropertyName("bidPr")]
    public string BidPrice { get; set; } = string.Empty;

    [JsonPropertyName("askPr")]
    public string AskPrice { get; set; } = string.Empty;

    [JsonPropertyName("high24h")]
    public string High24h { get; set; } = string.Empty;

    [JsonPropertyName("low24h")]
    public string Low24h { get; set; } = string.Empty;

    [JsonPropertyName("baseVolume")]
    public string BaseVolume { get; set; } = string.Empty;

    [JsonPropertyName("quoteVolume")]
    public string QuoteVolume { get; set; } = string.Empty;

    [JsonPropertyName("openUtc")]
    public string OpenUtc { get; set; } = string.Empty;

    [JsonPropertyName("ts")]
    public long Timestamp { get; set; }
}

public class FuturesTradeData
{
    [JsonPropertyName("instId")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("tradeId")]
    public string TradeId { get; set; } = string.Empty;

    [JsonPropertyName("px")]
    public string Price { get; set; } = string.Empty;

    [JsonPropertyName("sz")]
    public string Size { get; set; } = string.Empty;

    [JsonPropertyName("side")]
    public string Side { get; set; } = string.Empty;

    [JsonPropertyName("ts")]
    public long Timestamp { get; set; }
}

public class FuturesDepthData
{
    [JsonPropertyName("instId")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("asks")]
    public List<List<string>> Asks { get; set; } = new();

    [JsonPropertyName("bids")]
    public List<List<string>> Bids { get; set; } = new();

    [JsonPropertyName("ts")]
    public long Timestamp { get; set; }
}

public class FundingRateData
{
    [JsonPropertyName("instId")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("fundingRate")]
    public string FundingRate { get; set; } = string.Empty;

    [JsonPropertyName("fundingTime")]
    public long FundingTime { get; set; }
}

#endregion

/// <summary>
/// Futures public WebSocket channels
/// </summary>
public class FuturesPublicChannels
{
    private readonly BitgetWebSocketClient _webSocket;

    public FuturesPublicChannels(BitgetWebSocketClient webSocket)
    {
        _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
    }

    /// <summary>
    /// Subscribe to futures ticker updates
    /// </summary>
    public async Task SubscribeTickerAsync(string symbol, Action<FuturesTickerData> callback, CancellationToken cancellationToken = default)
    {
        var channel = "ticker";
        await _webSocket.SubscribeAsync(channel, symbol, "mc", isPrivate: false, cancellationToken);

        _webSocket.AddSubscription($"{channel}_futures_{symbol}", message =>
        {
            try
            {
                var response = JsonSerializer.Deserialize<WebSocketResponse<FuturesTickerData>>(message);
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
    /// Subscribe to futures trade updates
    /// </summary>
    public async Task SubscribeTradesAsync(string symbol, Action<FuturesTradeData> callback, CancellationToken cancellationToken = default)
    {
        var channel = "trade";
        await _webSocket.SubscribeAsync(channel, symbol, "mc", isPrivate: false, cancellationToken);

        _webSocket.AddSubscription($"{channel}_futures_{symbol}", message =>
        {
            try
            {
                var response = JsonSerializer.Deserialize<WebSocketResponse<FuturesTradeData>>(message);
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
    /// Subscribe to futures depth updates
    /// </summary>
    public async Task SubscribeDepthAsync(string symbol, Action<FuturesDepthData> callback, CancellationToken cancellationToken = default)
    {
        var channel = "books5";
        await _webSocket.SubscribeAsync(channel, symbol, "mc", isPrivate: false, cancellationToken);

        _webSocket.AddSubscription($"{channel}_futures_{symbol}", message =>
        {
            try
            {
                var response = JsonSerializer.Deserialize<WebSocketResponse<FuturesDepthData>>(message);
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
    /// Subscribe to funding rate updates
    /// </summary>
    public async Task SubscribeFundingRateAsync(string symbol, Action<FundingRateData> callback, CancellationToken cancellationToken = default)
    {
        var channel = "funding-rate";
        await _webSocket.SubscribeAsync(channel, symbol, "mc", isPrivate: false, cancellationToken);

        _webSocket.AddSubscription($"{channel}_{symbol}", message =>
        {
            try
            {
                var response = JsonSerializer.Deserialize<WebSocketResponse<FundingRateData>>(message);
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
