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
    public string Timestamp { get; set; } = string.Empty;

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

public class OrderBookData
{
    [JsonPropertyName("asks")]
    public List<List<string>> Asks { get; set; } = new();

    [JsonPropertyName("bids")]
    public List<List<string>> Bids { get; set; } = new();

    [JsonPropertyName("ts")]
    public string Timestamp { get; set; } = string.Empty;

    // Helper properties for easy access
    public List<(decimal Price, decimal Size)> AsksParsed =>
        Asks.Select(a => (decimal.Parse(a[0]), decimal.Parse(a[1]))).ToList();

    public List<(decimal Price, decimal Size)> BidsParsed =>
        Bids.Select(b => (decimal.Parse(b[0]), decimal.Parse(b[1]))).ToList();
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
    public string Timestamp { get; set; } = string.Empty;
}

/// <summary>
/// Candle data returned as array: [timestamp, open, high, low, close, baseVolume, quoteVolume]
/// </summary>
public class CandleData
{
    private List<string> _raw = new();

    // Constructor from array
    public CandleData(List<string> raw)
    {
        _raw = raw ?? new();
    }

    // For JSON deserialization
    public CandleData() { }

    // Bitget returns: [ts, open, high, low, close, baseVolume, quoteVolume]
    public long Timestamp => _raw.Count > 0 && long.TryParse(_raw[0], out var ts) ? ts : 0;
    public string Open => _raw.Count > 1 ? _raw[1] : "0";
    public string High => _raw.Count > 2 ? _raw[2] : "0";
    public string Low => _raw.Count > 3 ? _raw[3] : "0";
    public string Close => _raw.Count > 4 ? _raw[4] : "0";
    public string BaseVolume => _raw.Count > 5 ? _raw[5] : "0";
    public string QuoteVolume => _raw.Count > 6 ? _raw[6] : "0";

    // Internal setter for deserialization
    internal void SetRaw(List<string> raw) => _raw = raw;
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

        // Get all tickers and filter for the symbol
        var response = await _httpClient.GetAsync<List<TickerData>>($"/api/v2/spot/market/tickers", requiresAuth: false, cancellationToken);

        if (response.IsSuccess && response.Data != null)
        {
            var ticker = response.Data.FirstOrDefault(t => t.Symbol == symbol);
            if (ticker != null)
            {
                return new BitgetResponse<TickerData>
                {
                    Code = response.Code,
                    Message = response.Message,
                    RequestTime = response.RequestTime,
                    Data = ticker
                };
            }
        }

        return new BitgetResponse<TickerData>
        {
            Code = "40404",
            Message = $"Symbol {symbol} not found",
            RequestTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Data = null
        };
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
    public async Task<BitgetResponse<List<CandleData>>> GetCandlesticksAsync(
    string symbol,
    string granularity,
    long? startTime = null,
    long? endTime = null,
    CancellationToken cancellationToken = default)
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

        // Bitget returns List<List<string>>, we need to convert
        var response = await _httpClient.GetAsync<List<List<string>>>(endpoint, requiresAuth: false, cancellationToken);

        if (response.IsSuccess && response.Data != null)
        {
            var candles = response.Data.Select(raw => new CandleData(raw)).ToList();

            return new BitgetResponse<List<CandleData>>
            {
                Code = response.Code,
                Message = response.Message,
                RequestTime = response.RequestTime,
                Data = candles
            };
        }

        return new BitgetResponse<List<CandleData>>
        {
            Code = response.Code,
            Message = response.Message,
            RequestTime = response.RequestTime,
            Data = null
        };
    }
}
