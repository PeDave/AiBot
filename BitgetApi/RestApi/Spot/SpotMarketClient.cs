using System.Text.Json.Serialization;
using BitgetApi.Http;
using BitgetApi.Models;

namespace BitgetApi.RestApi.Spot;

#region Response Models

public class SymbolInfo
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("baseCoin")]
    public string BaseCoin { get; set; } = string.Empty;

    [JsonPropertyName("quoteCoin")]
    public string QuoteCoin { get; set; } = string.Empty;

    [JsonPropertyName("minTradeAmount")]
    public string MinTradeAmount { get; set; } = string.Empty;

    [JsonPropertyName("maxTradeAmount")]
    public string MaxTradeAmount { get; set; } = string.Empty;

    [JsonPropertyName("takerFeeRate")]
    public string TakerFeeRate { get; set; } = string.Empty;

    [JsonPropertyName("makerFeeRate")]
    public string MakerFeeRate { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public class TickerData
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("high24h")]
    public string High24h { get; set; } = string.Empty;

    [JsonPropertyName("low24h")]
    public string Low24h { get; set; } = string.Empty;

    [JsonPropertyName("open24h")]
    public string Open24h { get; set; } = string.Empty;

    [JsonPropertyName("lastPr")]
    public string LastPrice { get; set; } = string.Empty;

    [JsonPropertyName("quoteVolume")]
    public string QuoteVolume { get; set; } = string.Empty;

    [JsonPropertyName("baseVolume")]
    public string BaseVolume { get; set; } = string.Empty;

    [JsonPropertyName("usdtVolume")]
    public string UsdtVolume { get; set; } = string.Empty;

    [JsonPropertyName("ts")]
    public long Timestamp { get; set; }

    [JsonPropertyName("bidPr")]
    public string BidPrice { get; set; } = string.Empty;

    [JsonPropertyName("askPr")]
    public string AskPrice { get; set; } = string.Empty;

    [JsonPropertyName("bidSz")]
    public string BidSize { get; set; } = string.Empty;

    [JsonPropertyName("askSz")]
    public string AskSize { get; set; } = string.Empty;

    [JsonPropertyName("openUtc")]
    public string OpenUtc { get; set; } = string.Empty;

    [JsonPropertyName("changeUtc24h")]
    public string ChangeUtc24h { get; set; } = string.Empty;

    [JsonPropertyName("change24h")]
    public string Change24h { get; set; } = string.Empty;
}

public class OrderBookEntry
{
    [JsonPropertyName("price")]
    public string Price { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public string Size { get; set; } = string.Empty;
}

public class OrderBookData
{
    [JsonPropertyName("asks")]
    public List<OrderBookEntry> Asks { get; set; } = new();

    [JsonPropertyName("bids")]
    public List<OrderBookEntry> Bids { get; set; } = new();

    [JsonPropertyName("ts")]
    public long Timestamp { get; set; }
}

public class TradeData
{
    [JsonPropertyName("tradeId")]
    public string TradeId { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public string Price { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public string Size { get; set; } = string.Empty;

    [JsonPropertyName("side")]
    public string Side { get; set; } = string.Empty;

    [JsonPropertyName("ts")]
    public long Timestamp { get; set; }
}

public class CandleData
{
    [JsonPropertyName("ts")]
    public long Timestamp { get; set; }

    [JsonPropertyName("open")]
    public string Open { get; set; } = string.Empty;

    [JsonPropertyName("high")]
    public string High { get; set; } = string.Empty;

    [JsonPropertyName("low")]
    public string Low { get; set; } = string.Empty;

    [JsonPropertyName("close")]
    public string Close { get; set; } = string.Empty;

    [JsonPropertyName("baseVolume")]
    public string BaseVolume { get; set; } = string.Empty;

    [JsonPropertyName("quoteVolume")]
    public string QuoteVolume { get; set; } = string.Empty;
}

#endregion

/// <summary>
/// Client for Spot Market public endpoints
/// </summary>
public class SpotMarketClient
{
    private readonly BitgetHttpClient _httpClient;

    public SpotMarketClient(BitgetHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Get all spot symbols
    /// </summary>
    public async Task<BitgetResponse<List<SymbolInfo>>> GetSymbolsAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<SymbolInfo>>("/api/v2/spot/public/symbols", requiresAuth: false, cancellationToken);
    }

    /// <summary>
    /// Get ticker for a specific symbol
    /// </summary>
    public async Task<BitgetResponse<TickerData>> GetTickerAsync(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty", nameof(symbol));

        return await _httpClient.GetAsync<TickerData>($"/api/v2/spot/market/ticker?symbol={symbol}", requiresAuth: false, cancellationToken);
    }

    /// <summary>
    /// Get tickers for all symbols
    /// </summary>
    public async Task<BitgetResponse<List<TickerData>>> GetTickersAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<TickerData>>("/api/v2/spot/market/tickers", requiresAuth: false, cancellationToken);
    }

    /// <summary>
    /// Get market depth (order book)
    /// </summary>
    public async Task<BitgetResponse<OrderBookData>> GetMarketDepthAsync(string symbol, int limit = 100, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty", nameof(symbol));

        return await _httpClient.GetAsync<OrderBookData>($"/api/v2/spot/market/orderbook?symbol={symbol}&limit={limit}", requiresAuth: false, cancellationToken);
    }

    /// <summary>
    /// Get recent trades
    /// </summary>
    public async Task<BitgetResponse<List<TradeData>>> GetRecentTradesAsync(string symbol, int limit = 100, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty", nameof(symbol));

        return await _httpClient.GetAsync<List<TradeData>>($"/api/v2/spot/market/fills?symbol={symbol}&limit={limit}", requiresAuth: false, cancellationToken);
    }

    /// <summary>
    /// Get candlestick data
    /// </summary>
    /// <param name="symbol">Trading pair symbol</param>
    /// <param name="granularity">Candle granularity (1min, 5min, 15min, 30min, 1h, 4h, 6h, 12h, 1day, 1week)</param>
    /// <param name="startTime">Start time in milliseconds</param>
    /// <param name="endTime">End time in milliseconds</param>
    public async Task<BitgetResponse<List<CandleData>>> GetCandlesticksAsync(string symbol, string granularity, long? startTime = null, long? endTime = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty", nameof(symbol));

        if (string.IsNullOrWhiteSpace(granularity))
            throw new ArgumentException("Granularity cannot be empty", nameof(granularity));

        var endpoint = $"/api/v2/spot/market/candles?symbol={symbol}&granularity={granularity}";
        
        if (startTime.HasValue)
            endpoint += $"&startTime={startTime.Value}";
        
        if (endTime.HasValue)
            endpoint += $"&endTime={endTime.Value}";

        return await _httpClient.GetAsync<List<CandleData>>(endpoint, requiresAuth: false, cancellationToken);
    }
}
