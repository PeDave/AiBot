using System.Text.Json.Serialization;
using BitgetApi.Http;
using BitgetApi.Models;

namespace BitgetApi.RestApi.Futures;

#region Response Models

public class ContractInfo
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("baseCoin")]
    public string BaseCoin { get; set; } = string.Empty;

    [JsonPropertyName("quoteCoin")]
    public string QuoteCoin { get; set; } = string.Empty;

    [JsonPropertyName("buyLimitPriceRatio")]
    public string BuyLimitPriceRatio { get; set; } = string.Empty;

    [JsonPropertyName("sellLimitPriceRatio")]
    public string SellLimitPriceRatio { get; set; } = string.Empty;

    [JsonPropertyName("feeRateUpRatio")]
    public string FeeRateUpRatio { get; set; } = string.Empty;

    [JsonPropertyName("makerFeeRate")]
    public string MakerFeeRate { get; set; } = string.Empty;

    [JsonPropertyName("takerFeeRate")]
    public string TakerFeeRate { get; set; } = string.Empty;

    [JsonPropertyName("openCostUpRatio")]
    public string OpenCostUpRatio { get; set; } = string.Empty;

    [JsonPropertyName("supportMarginCoins")]
    public List<string> SupportMarginCoins { get; set; } = new();

    [JsonPropertyName("minTradeNum")]
    public string MinTradeNum { get; set; } = string.Empty;

    [JsonPropertyName("priceEndStep")]
    public string PriceEndStep { get; set; } = string.Empty;

    [JsonPropertyName("volumePlace")]
    public string VolumePlace { get; set; } = string.Empty;

    [JsonPropertyName("pricePlace")]
    public string PricePlace { get; set; } = string.Empty;

    [JsonPropertyName("sizeMultiplier")]
    public string SizeMultiplier { get; set; } = string.Empty;

    [JsonPropertyName("symbolType")]
    public string SymbolType { get; set; } = string.Empty;

    [JsonPropertyName("minTradeUSDT")]
    public string MinTradeUSDT { get; set; } = string.Empty;

    [JsonPropertyName("maxSymbolOpenNum")]
    public string MaxSymbolOpenNum { get; set; } = string.Empty;

    [JsonPropertyName("maxSymbolOrderNum")]
    public string MaxSymbolOrderNum { get; set; } = string.Empty;

    [JsonPropertyName("maxPositionNum")]
    public string MaxPositionNum { get; set; } = string.Empty;

    [JsonPropertyName("symbolStatus")]
    public string SymbolStatus { get; set; } = string.Empty;

    [JsonPropertyName("offTime")]
    public string OffTime { get; set; } = string.Empty;

    [JsonPropertyName("limitOpenTime")]
    public string LimitOpenTime { get; set; } = string.Empty;

    [JsonPropertyName("deliveryTime")]
    public string DeliveryTime { get; set; } = string.Empty;

    [JsonPropertyName("deliveryStatus")]
    public string DeliveryStatus { get; set; } = string.Empty;

    [JsonPropertyName("deliveryPeriod")]
    public string DeliveryPeriod { get; set; } = string.Empty;
}

public class FuturesTickerData
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("lastPr")]
    public string LastPrice { get; set; } = string.Empty;

    [JsonPropertyName("askPr")]
    public string AskPrice { get; set; } = string.Empty;

    [JsonPropertyName("bidPr")]
    public string BidPrice { get; set; } = string.Empty;

    [JsonPropertyName("askSz")]
    public string AskSize { get; set; } = string.Empty;

    [JsonPropertyName("bidSz")]
    public string BidSize { get; set; } = string.Empty;

    [JsonPropertyName("high24h")]
    public string High24h { get; set; } = string.Empty;

    [JsonPropertyName("low24h")]
    public string Low24h { get; set; } = string.Empty;

    [JsonPropertyName("ts")]
    public long Timestamp { get; set; }

    [JsonPropertyName("change24h")]
    public string Change24h { get; set; } = string.Empty;

    [JsonPropertyName("baseVolume")]
    public string BaseVolume { get; set; } = string.Empty;

    [JsonPropertyName("quoteVolume")]
    public string QuoteVolume { get; set; } = string.Empty;

    [JsonPropertyName("usdtVolume")]
    public string UsdtVolume { get; set; } = string.Empty;

    [JsonPropertyName("openUtc")]
    public string OpenUtc { get; set; } = string.Empty;

    [JsonPropertyName("changeUtc24h")]
    public string ChangeUtc24h { get; set; } = string.Empty;

    [JsonPropertyName("indexPrice")]
    public string IndexPrice { get; set; } = string.Empty;

    [JsonPropertyName("fundingRate")]
    public string FundingRate { get; set; } = string.Empty;

    [JsonPropertyName("holdingAmount")]
    public string HoldingAmount { get; set; } = string.Empty;
}

public class FuturesOrderBookData
{
    [JsonPropertyName("asks")]
    public List<List<string>> Asks { get; set; } = new();

    [JsonPropertyName("bids")]
    public List<List<string>> Bids { get; set; } = new();

    [JsonPropertyName("ts")]
    public long Timestamp { get; set; }
}

public class FuturesCandleData
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

    [JsonPropertyName("usdtVolume")]
    public string UsdtVolume { get; set; } = string.Empty;
}

public class FundingRateData
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("fundingRate")]
    public string FundingRate { get; set; } = string.Empty;

    [JsonPropertyName("fundingTime")]
    public long FundingTime { get; set; }
}

public class HistoricalFundingRate
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("fundingRate")]
    public string FundingRate { get; set; } = string.Empty;

    [JsonPropertyName("settleTime")]
    public long SettleTime { get; set; }
}

#endregion

/// <summary>
/// Client for Futures Market public endpoints
/// </summary>
public class FuturesMarketClient
{
    private readonly BitgetHttpClient _httpClient;

    public FuturesMarketClient(BitgetHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Get futures contracts
    /// </summary>
    public async Task<BitgetResponse<List<ContractInfo>>> GetContractsAsync(string productType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productType))
            throw new ArgumentException("Product type cannot be empty", nameof(productType));

        return await _httpClient.GetAsync<List<ContractInfo>>($"/api/v2/mix/market/contracts?productType={productType}", requiresAuth: false, cancellationToken);
    }

    /// <summary>
    /// Get ticker for a specific symbol
    /// </summary>
    public async Task<BitgetResponse<FuturesTickerData>> GetTickerAsync(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty", nameof(symbol));

        return await _httpClient.GetAsync<FuturesTickerData>($"/api/v2/mix/market/ticker?symbol={symbol}", requiresAuth: false, cancellationToken);
    }

    /// <summary>
    /// Get market depth (order book)
    /// </summary>
    public async Task<BitgetResponse<FuturesOrderBookData>> GetMarketDepthAsync(string symbol, int limit = 100, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty", nameof(symbol));

        return await _httpClient.GetAsync<FuturesOrderBookData>($"/api/v2/mix/market/orderbook?symbol={symbol}&limit={limit}", requiresAuth: false, cancellationToken);
    }

    /// <summary>
    /// Get candlestick data
    /// </summary>
    public async Task<BitgetResponse<List<FuturesCandleData>>> GetCandlesticksAsync(string symbol, string granularity, long? startTime = null, long? endTime = null, int limit = 100, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty", nameof(symbol));

        if (string.IsNullOrWhiteSpace(granularity))
            throw new ArgumentException("Granularity cannot be empty", nameof(granularity));

        var endpoint = $"/api/v2/mix/market/candles?symbol={symbol}&granularity={granularity}&limit={limit}";
        
        if (startTime.HasValue)
            endpoint += $"&startTime={startTime.Value}";
        
        if (endTime.HasValue)
            endpoint += $"&endTime={endTime.Value}";

        return await _httpClient.GetAsync<List<FuturesCandleData>>(endpoint, requiresAuth: false, cancellationToken);
    }

    /// <summary>
    /// Get funding rate
    /// </summary>
    public async Task<BitgetResponse<FundingRateData>> GetFundingRateAsync(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty", nameof(symbol));

        return await _httpClient.GetAsync<FundingRateData>($"/api/v2/mix/market/funding-time?symbol={symbol}", requiresAuth: false, cancellationToken);
    }

    /// <summary>
    /// Get historical funding rates
    /// </summary>
    public async Task<BitgetResponse<List<HistoricalFundingRate>>> GetHistoricalFundingRatesAsync(string symbol, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty", nameof(symbol));

        return await _httpClient.GetAsync<List<HistoricalFundingRate>>($"/api/v2/mix/market/history-fund-rate?symbol={symbol}&pageSize={pageSize}", requiresAuth: false, cancellationToken);
    }
}
