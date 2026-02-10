using System.Text.Json.Serialization;
using BitgetApi.Http;
using BitgetApi.Models;

namespace BitgetApi.RestApi.Futures;

#region Response Models

public class FuturesAccountInfo
{
    [JsonPropertyName("marginCoin")]
    public string MarginCoin { get; set; } = string.Empty;

    [JsonPropertyName("locked")]
    public string Locked { get; set; } = string.Empty;

    [JsonPropertyName("available")]
    public string Available { get; set; } = string.Empty;

    [JsonPropertyName("crossMaxAvailable")]
    public string CrossMaxAvailable { get; set; } = string.Empty;

    [JsonPropertyName("isolatedMaxAvailable")]
    public string IsolatedMaxAvailable { get; set; } = string.Empty;

    [JsonPropertyName("maxTransferOut")]
    public string MaxTransferOut { get; set; } = string.Empty;

    [JsonPropertyName("equity")]
    public string Equity { get; set; } = string.Empty;

    [JsonPropertyName("usdtEquity")]
    public string UsdtEquity { get; set; } = string.Empty;
}

public class FuturesPositionInfo
{
    [JsonPropertyName("marginCoin")]
    public string MarginCoin { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("holdSide")]
    public string HoldSide { get; set; } = string.Empty;

    [JsonPropertyName("openDelegateSize")]
    public string OpenDelegateSize { get; set; } = string.Empty;

    [JsonPropertyName("marginSize")]
    public string MarginSize { get; set; } = string.Empty;

    [JsonPropertyName("available")]
    public string Available { get; set; } = string.Empty;

    [JsonPropertyName("locked")]
    public string Locked { get; set; } = string.Empty;

    [JsonPropertyName("total")]
    public string Total { get; set; } = string.Empty;

    [JsonPropertyName("leverage")]
    public string Leverage { get; set; } = string.Empty;

    [JsonPropertyName("achievedProfits")]
    public string AchievedProfits { get; set; } = string.Empty;

    [JsonPropertyName("openPriceAvg")]
    public string OpenPriceAvg { get; set; } = string.Empty;

    [JsonPropertyName("marginMode")]
    public string MarginMode { get; set; } = string.Empty;

    [JsonPropertyName("holdMode")]
    public string HoldMode { get; set; } = string.Empty;

    [JsonPropertyName("unrealizedPL")]
    public string UnrealizedPL { get; set; } = string.Empty;

    [JsonPropertyName("liquidationPrice")]
    public string LiquidationPrice { get; set; } = string.Empty;

    [JsonPropertyName("keepMarginRate")]
    public string KeepMarginRate { get; set; } = string.Empty;

    [JsonPropertyName("marketPrice")]
    public string MarketPrice { get; set; } = string.Empty;

    [JsonPropertyName("cTime")]
    public long CreateTime { get; set; }

    [JsonPropertyName("uTime")]
    public long UpdateTime { get; set; }
}

public class LeverageResponse
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("marginCoin")]
    public string MarginCoin { get; set; } = string.Empty;

    [JsonPropertyName("longLeverage")]
    public string LongLeverage { get; set; } = string.Empty;

    [JsonPropertyName("shortLeverage")]
    public string ShortLeverage { get; set; } = string.Empty;

    [JsonPropertyName("crossMarginLeverage")]
    public string CrossMarginLeverage { get; set; } = string.Empty;

    [JsonPropertyName("isolatedLongLeverage")]
    public string IsolatedLongLeverage { get; set; } = string.Empty;

    [JsonPropertyName("isolatedShortLeverage")]
    public string IsolatedShortLeverage { get; set; } = string.Empty;
}

public class MarginModeResponse
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("marginCoin")]
    public string MarginCoin { get; set; } = string.Empty;

    [JsonPropertyName("marginMode")]
    public string MarginMode { get; set; } = string.Empty;
}

public class PositionModeResponse
{
    [JsonPropertyName("productType")]
    public string ProductType { get; set; } = string.Empty;

    [JsonPropertyName("holdMode")]
    public string HoldMode { get; set; } = string.Empty;
}

#endregion

/// <summary>
/// Client for Futures Account private endpoints
/// </summary>
public class FuturesAccountClient
{
    private readonly BitgetHttpClient _httpClient;

    public FuturesAccountClient(BitgetHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Get single futures account
    /// </summary>
    public async Task<BitgetResponse<FuturesAccountInfo>> GetAccountAsync(string symbol, string marginCoin, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty", nameof(symbol));
        
        if (string.IsNullOrWhiteSpace(marginCoin))
            throw new ArgumentException("Margin coin cannot be empty", nameof(marginCoin));

        return await _httpClient.GetAsync<FuturesAccountInfo>($"/api/v2/mix/account/account?symbol={symbol}&marginCoin={marginCoin}", requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Get all futures accounts
    /// </summary>
    public async Task<BitgetResponse<List<FuturesAccountInfo>>> GetAccountsAsync(string productType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productType))
            throw new ArgumentException("Product type cannot be empty", nameof(productType));

        return await _httpClient.GetAsync<List<FuturesAccountInfo>>($"/api/v2/mix/account/accounts?productType={productType}", requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Get single position
    /// </summary>
    public async Task<BitgetResponse<List<FuturesPositionInfo>>> GetPositionsAsync(string symbol, string marginCoin, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            throw new ArgumentException("Symbol cannot be empty", nameof(symbol));
        
        if (string.IsNullOrWhiteSpace(marginCoin))
            throw new ArgumentException("Margin coin cannot be empty", nameof(marginCoin));

        return await _httpClient.GetAsync<List<FuturesPositionInfo>>($"/api/v2/mix/position/single-position?symbol={symbol}&marginCoin={marginCoin}", requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Get all positions
    /// </summary>
    public async Task<BitgetResponse<List<FuturesPositionInfo>>> GetAllPositionsAsync(string productType, string marginCoin, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(productType))
            throw new ArgumentException("Product type cannot be empty", nameof(productType));
        
        if (string.IsNullOrWhiteSpace(marginCoin))
            throw new ArgumentException("Margin coin cannot be empty", nameof(marginCoin));

        return await _httpClient.GetAsync<List<FuturesPositionInfo>>($"/api/v2/mix/position/all-position?productType={productType}&marginCoin={marginCoin}", requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Set leverage
    /// </summary>
    public async Task<BitgetResponse<LeverageResponse>> SetLeverageAsync(string symbol, string marginCoin, int leverage, string? holdSide = null, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            symbol,
            marginCoin,
            leverage = leverage.ToString(),
            holdSide
        };

        return await _httpClient.PostAsync<LeverageResponse>("/api/v2/mix/account/set-leverage", request, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Set margin mode
    /// </summary>
    public async Task<BitgetResponse<MarginModeResponse>> SetMarginModeAsync(string symbol, string marginCoin, string marginMode, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            symbol,
            marginCoin,
            marginMode
        };

        return await _httpClient.PostAsync<MarginModeResponse>("/api/v2/mix/account/set-margin-mode", request, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Set position mode
    /// </summary>
    public async Task<BitgetResponse<PositionModeResponse>> SetPositionModeAsync(string productType, string positionMode, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            productType,
            holdMode = positionMode
        };

        return await _httpClient.PostAsync<PositionModeResponse>("/api/v2/mix/account/set-position-mode", request, requiresAuth: true, cancellationToken);
    }
}
