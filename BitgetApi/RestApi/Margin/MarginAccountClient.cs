using System.Text.Json.Serialization;
using BitgetApi.Http;
using BitgetApi.Models;

namespace BitgetApi.RestApi.Margin;

#region Response Models

public class CrossMarginAsset
{
    [JsonPropertyName("coin")]
    public string Coin { get; set; } = string.Empty;

    [JsonPropertyName("available")]
    public string Available { get; set; } = string.Empty;

    [JsonPropertyName("frozen")]
    public string Frozen { get; set; } = string.Empty;

    [JsonPropertyName("borrowed")]
    public string Borrowed { get; set; } = string.Empty;

    [JsonPropertyName("interest")]
    public string Interest { get; set; } = string.Empty;

    [JsonPropertyName("net")]
    public string Net { get; set; } = string.Empty;
}

public class IsolatedMarginAsset
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("baseCoin")]
    public string BaseCoin { get; set; } = string.Empty;

    [JsonPropertyName("quoteCoin")]
    public string QuoteCoin { get; set; } = string.Empty;

    [JsonPropertyName("baseAvailable")]
    public string BaseAvailable { get; set; } = string.Empty;

    [JsonPropertyName("quoteAvailable")]
    public string QuoteAvailable { get; set; } = string.Empty;

    [JsonPropertyName("baseBorrowed")]
    public string BaseBorrowed { get; set; } = string.Empty;

    [JsonPropertyName("quoteBorrowed")]
    public string QuoteBorrowed { get; set; } = string.Empty;

    [JsonPropertyName("baseInterest")]
    public string BaseInterest { get; set; } = string.Empty;

    [JsonPropertyName("quoteInterest")]
    public string QuoteInterest { get; set; } = string.Empty;
}

public class BorrowResponse
{
    [JsonPropertyName("borrowId")]
    public string BorrowId { get; set; } = string.Empty;

    [JsonPropertyName("coin")]
    public string Coin { get; set; } = string.Empty;

    [JsonPropertyName("borrowAmount")]
    public string BorrowAmount { get; set; } = string.Empty;
}

public class RepayResponse
{
    [JsonPropertyName("repayId")]
    public string RepayId { get; set; } = string.Empty;

    [JsonPropertyName("coin")]
    public string Coin { get; set; } = string.Empty;

    [JsonPropertyName("repayAmount")]
    public string RepayAmount { get; set; } = string.Empty;
}

#endregion

/// <summary>
/// Client for Margin Account private endpoints
/// </summary>
public class MarginAccountClient
{
    private readonly BitgetHttpClient _httpClient;

    public MarginAccountClient(BitgetHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Get cross margin account assets
    /// </summary>
    public async Task<BitgetResponse<List<CrossMarginAsset>>> GetCrossMarginAccountAssetsAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<CrossMarginAsset>>("/api/v2/margin/crossed/account/assets", requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Get isolated margin account assets
    /// </summary>
    public async Task<BitgetResponse<List<IsolatedMarginAsset>>> GetIsolatedMarginAccountAssetsAsync(string? symbol = null, CancellationToken cancellationToken = default)
    {
        var endpoint = "/api/v2/margin/isolated/account/assets";
        
        if (!string.IsNullOrWhiteSpace(symbol))
            endpoint += $"?symbol={symbol}";

        return await _httpClient.GetAsync<List<IsolatedMarginAsset>>(endpoint, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Borrow in cross margin
    /// </summary>
    public async Task<BitgetResponse<BorrowResponse>> BorrowAsync(string coin, string amount, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            coin,
            amount
        };

        return await _httpClient.PostAsync<BorrowResponse>("/api/v2/margin/crossed/account/borrow", request, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Repay in cross margin
    /// </summary>
    public async Task<BitgetResponse<RepayResponse>> RepayAsync(string coin, string amount, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            coin,
            amount
        };

        return await _httpClient.PostAsync<RepayResponse>("/api/v2/margin/crossed/account/repay", request, requiresAuth: true, cancellationToken);
    }
}
