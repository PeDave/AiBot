using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace BitgetApi.WebSocket.Public;

#region Data Models

public class TickerData
{
    [JsonPropertyName("instId")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("lastPr")]
    public string LastPrice { get; set; } = string.Empty;

    [JsonPropertyName("open24h")]
    public string Open24h { get; set; } = string.Empty;

    [JsonPropertyName("high24h")]
    public string High24h { get; set; } = string.Empty;

    [JsonPropertyName("low24h")]
    public string Low24h { get; set; } = string.Empty;

    [JsonPropertyName("bidPr")]
    public string BidPrice { get; set; } = string.Empty;

    [JsonPropertyName("askPr")]
    public string AskPrice { get; set; } = string.Empty;

    [JsonPropertyName("baseVolume")]
    public string BaseVolume { get; set; } = string.Empty;

    [JsonPropertyName("quoteVolume")]
    public string QuoteVolume { get; set; } = string.Empty;

    [JsonPropertyName("ts")]
    public long Timestamp { get; set; }
}

public class TradeData
{
    [JsonPropertyName("instId")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("tradeId")]
    public string TradeId { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public string Price { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public string Size { get; set; } = string.Empty;

    [JsonPropertyName("side")]
    public string Side { get; set; } = string.Empty;

    [JsonPropertyName("ts")]
    public string Timestamp { get; set; } = string.Empty;
}

public class DepthData
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

public class CandleData
{
    [JsonPropertyName("instId")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("ts")]
    public long Timestamp { get; set; }

    [JsonPropertyName("o")]
    public string Open { get; set; } = string.Empty;

    [JsonPropertyName("h")]
    public string High { get; set; } = string.Empty;

    [JsonPropertyName("l")]
    public string Low { get; set; } = string.Empty;

    [JsonPropertyName("c")]
    public string Close { get; set; } = string.Empty;

    [JsonPropertyName("vol")]
    public string Volume { get; set; } = string.Empty;

    [JsonPropertyName("volCcy")]
    public string VolumeInCurrency { get; set; } = string.Empty;
}

#endregion

/// <summary>
/// Spot public WebSocket channels
/// </summary>
public class SpotPublicChannels
{
    private readonly BitgetWebSocketClient _webSocket;
    private readonly ILogger<SpotPublicChannels>? _logger;

    public SpotPublicChannels(BitgetWebSocketClient webSocket, ILogger<SpotPublicChannels>? logger = null)
    {
        _webSocket = webSocket ?? throw new ArgumentNullException(nameof(webSocket));
        _logger = logger;
    }

    /// <summary>
    /// Subscribe to ticker updates
    /// </summary>
    public async Task SubscribeTickerAsync(string symbol, Action<TickerData> callback, CancellationToken cancellationToken = default)
    {
        var channel = "ticker";
        await _webSocket.SubscribeAsync(channel, symbol, "SPOT", isPrivate: false, cancellationToken);

        _webSocket.AddSubscription($"{channel}_{symbol}", message =>
        {
            try
            {
                var response = JsonSerializer.Deserialize<WebSocketResponse<TickerData>>(message);
                if (response?.Data != null)
                {
                    foreach (var data in response.Data)
                    {
                        callback(data);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error parsing ticker data for {Symbol}", symbol);
            }
        });
    }

    /// <summary>
    /// Subscribe to trade updates
    /// </summary>
    public async Task SubscribeTradesAsync(string symbol, Action<TradeData> callback, CancellationToken cancellationToken = default)
    {
        var channel = "trade";
        await _webSocket.SubscribeAsync(channel, symbol, "SPOT", isPrivate: false, cancellationToken);

        _webSocket.AddSubscription($"{channel}_{symbol}", message =>
        {
            try
            {
                var response = JsonSerializer.Deserialize<WebSocketResponse<TradeData>>(message);
                if (response?.Data != null)
                {
                    foreach (var data in response.Data)
                    {
                        callback(data);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error parsing trade data for {Symbol}", symbol);
            }
        });
    }

    /// <summary>
    /// Subscribe to order book depth updates
    /// </summary>
    public async Task SubscribeDepthAsync(string symbol, int depth, Action<DepthData> callback, CancellationToken cancellationToken = default)
    {
        var channel = depth <= 5 ? "books5" : "books15";
        await _webSocket.SubscribeAsync(channel, symbol, "SPOT", isPrivate: false, cancellationToken);

        _webSocket.AddSubscription($"{channel}_{symbol}", message =>
        {
            try
            {
                var response = JsonSerializer.Deserialize<WebSocketResponse<DepthData>>(message);
                if (response?.Data != null)
                {
                    foreach (var data in response.Data)
                    {
                        callback(data);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error parsing depth data for {Symbol}", symbol);
            }
        });
    }

    /// <summary>
    /// Subscribe to candlestick updates
    /// </summary>
    public async Task SubscribeCandlesAsync(string symbol, string interval, Action<CandleData> callback, CancellationToken cancellationToken = default)
    {
        var channel = $"candle{interval}";
        await _webSocket.SubscribeAsync(channel, symbol, "SPOT", isPrivate: false, cancellationToken);

        _webSocket.AddSubscription($"{channel}_{symbol}", message =>
        {
            try
            {
                var response = JsonSerializer.Deserialize<WebSocketResponse<CandleData>>(message);
                if (response?.Data != null)
                {
                    foreach (var data in response.Data)
                    {
                        callback(data);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error parsing candle data for {Symbol}", symbol);
            }
        });
    }
}
