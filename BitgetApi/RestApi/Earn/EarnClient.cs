using System.Text.Json.Serialization;
using BitgetApi.Http;
using BitgetApi.Models;

namespace BitgetApi.RestApi.Earn;

#region Response Models

public class SavingsProduct
{
    [JsonPropertyName("productId")]
    public string ProductId { get; set; } = string.Empty;

    [JsonPropertyName("coin")]
    public string Coin { get; set; } = string.Empty;

    [JsonPropertyName("period")]
    public string Period { get; set; } = string.Empty;

    [JsonPropertyName("periodUnit")]
    public string PeriodUnit { get; set; } = string.Empty;

    [JsonPropertyName("annualRate")]
    public string AnnualRate { get; set; } = string.Empty;

    [JsonPropertyName("minPurchaseAmount")]
    public string MinPurchaseAmount { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public class SubscribeResponse
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("productId")]
    public string ProductId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;
}

public class RedeemResponse
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;
}

public class StakingProduct
{
    [JsonPropertyName("productId")]
    public string ProductId { get; set; } = string.Empty;

    [JsonPropertyName("coin")]
    public string Coin { get; set; } = string.Empty;

    [JsonPropertyName("period")]
    public string Period { get; set; } = string.Empty;

    [JsonPropertyName("annualRate")]
    public string AnnualRate { get; set; } = string.Empty;

    [JsonPropertyName("minStakeAmount")]
    public string MinStakeAmount { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

#endregion

/// <summary>
/// Client for Earn (Savings & Staking) private endpoints
/// </summary>
public class EarnClient
{
    private readonly BitgetHttpClient _httpClient;

    public EarnClient(BitgetHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Get savings products
    /// </summary>
    public async Task<BitgetResponse<List<SavingsProduct>>> GetSavingsProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<SavingsProduct>>("/api/v2/earn/savings/products", requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Subscribe to savings
    /// </summary>
    public async Task<BitgetResponse<SubscribeResponse>> SubscribeSavingsAsync(string productId, string amount, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            productId,
            amount
        };

        return await _httpClient.PostAsync<SubscribeResponse>("/api/v2/earn/savings/subscribe", request, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Redeem from savings
    /// </summary>
    public async Task<BitgetResponse<RedeemResponse>> RedeemSavingsAsync(string orderId, string amount, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            orderId,
            amount
        };

        return await _httpClient.PostAsync<RedeemResponse>("/api/v2/earn/savings/redeem", request, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Get staking products
    /// </summary>
    public async Task<BitgetResponse<List<StakingProduct>>> GetStakingProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<StakingProduct>>("/api/v2/earn/staking/products", requiresAuth: true, cancellationToken);
    }
}
