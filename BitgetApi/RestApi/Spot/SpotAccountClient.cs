using System.Text.Json.Serialization;
using BitgetApi.Http;
using BitgetApi.Models;

namespace BitgetApi.RestApi.Spot;

#region Response Models

public class AssetInfo
{
    [JsonPropertyName("coin")]
    public string Coin { get; set; } = string.Empty;

    [JsonPropertyName("available")]
    public string Available { get; set; } = string.Empty;

    [JsonPropertyName("frozen")]
    public string Frozen { get; set; } = string.Empty;

    [JsonPropertyName("locked")]
    public string Locked { get; set; } = string.Empty;

    [JsonPropertyName("usdtValue")]
    public string UsdtValue { get; set; } = string.Empty;
}

public class BillInfo
{
    [JsonPropertyName("billId")]
    public string BillId { get; set; } = string.Empty;

    [JsonPropertyName("coin")]
    public string Coin { get; set; } = string.Empty;

    [JsonPropertyName("groupType")]
    public string GroupType { get; set; } = string.Empty;

    [JsonPropertyName("businessType")]
    public string BusinessType { get; set; } = string.Empty;

    [JsonPropertyName("size")]
    public string Size { get; set; } = string.Empty;

    [JsonPropertyName("balance")]
    public string Balance { get; set; } = string.Empty;

    [JsonPropertyName("fees")]
    public string Fees { get; set; } = string.Empty;

    [JsonPropertyName("cTime")]
    public long CreateTime { get; set; }
}

public class SubAccountAsset
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("coin")]
    public string Coin { get; set; } = string.Empty;

    [JsonPropertyName("available")]
    public string Available { get; set; } = string.Empty;

    [JsonPropertyName("frozen")]
    public string Frozen { get; set; } = string.Empty;

    [JsonPropertyName("locked")]
    public string Locked { get; set; } = string.Empty;
}

#endregion

/// <summary>
/// Client for Spot Account private endpoints
/// </summary>
public class SpotAccountClient
{
    private readonly BitgetHttpClient _httpClient;

    public SpotAccountClient(BitgetHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Get account assets
    /// </summary>
    public async Task<BitgetResponse<List<AssetInfo>>> GetAccountAssetsAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<AssetInfo>>("/api/v2/spot/account/assets", requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Get account bills (transaction history)
    /// </summary>
    public async Task<BitgetResponse<List<BillInfo>>> GetAccountBillsAsync(string? coin = null, int limit = 100, CancellationToken cancellationToken = default)
    {
        var endpoint = $"/api/v2/spot/account/bills?limit={limit}";
        
        if (!string.IsNullOrWhiteSpace(coin))
            endpoint += $"&coin={coin}";

        return await _httpClient.GetAsync<List<BillInfo>>(endpoint, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Get sub-account spot assets
    /// </summary>
    public async Task<BitgetResponse<List<SubAccountAsset>>> GetSubAccountSpotAssetsAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<SubAccountAsset>>("/api/v2/spot/account/subaccount-assets", requiresAuth: true, cancellationToken);
    }
}
