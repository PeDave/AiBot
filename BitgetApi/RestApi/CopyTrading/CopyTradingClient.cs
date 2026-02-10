using System.Text.Json.Serialization;
using BitgetApi.Http;
using BitgetApi.Models;

namespace BitgetApi.RestApi.CopyTrading;

#region Response Models

public class TraderInfo
{
    [JsonPropertyName("traderId")]
    public string TraderId { get; set; } = string.Empty;

    [JsonPropertyName("traderName")]
    public string TraderName { get; set; } = string.Empty;

    [JsonPropertyName("followers")]
    public int Followers { get; set; }

    [JsonPropertyName("roi")]
    public string ROI { get; set; } = string.Empty;

    [JsonPropertyName("pnl")]
    public string PnL { get; set; } = string.Empty;

    [JsonPropertyName("winRate")]
    public string WinRate { get; set; } = string.Empty;
}

public class FollowResponse
{
    [JsonPropertyName("traderId")]
    public string TraderId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public class UnfollowResponse
{
    [JsonPropertyName("traderId")]
    public string TraderId { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public class FollowingTrader
{
    [JsonPropertyName("traderId")]
    public string TraderId { get; set; } = string.Empty;

    [JsonPropertyName("traderName")]
    public string TraderName { get; set; } = string.Empty;

    [JsonPropertyName("settleToken")]
    public string SettleToken { get; set; } = string.Empty;

    [JsonPropertyName("followTime")]
    public long FollowTime { get; set; }

    [JsonPropertyName("pnl")]
    public string PnL { get; set; } = string.Empty;
}

#endregion

/// <summary>
/// Client for Copy Trading private endpoints
/// </summary>
public class CopyTradingClient
{
    private readonly BitgetHttpClient _httpClient;

    public CopyTradingClient(BitgetHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Get available traders
    /// </summary>
    public async Task<BitgetResponse<List<TraderInfo>>> GetTradersAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<TraderInfo>>("/api/v2/copy/mix-trader/traders", requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Follow a trader
    /// </summary>
    public async Task<BitgetResponse<FollowResponse>> FollowTraderAsync(string traderId, string settleToken, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            traderId,
            settleToken
        };

        return await _httpClient.PostAsync<FollowResponse>("/api/v2/copy/mix-follower/follow-trader", request, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Unfollow a trader
    /// </summary>
    public async Task<BitgetResponse<UnfollowResponse>> UnfollowTraderAsync(string traderId, CancellationToken cancellationToken = default)
    {
        var request = new
        {
            traderId
        };

        return await _httpClient.PostAsync<UnfollowResponse>("/api/v2/copy/mix-follower/unfollow-trader", request, requiresAuth: true, cancellationToken);
    }

    /// <summary>
    /// Get following traders
    /// </summary>
    public async Task<BitgetResponse<List<FollowingTrader>>> GetFollowingTradersAsync(CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetAsync<List<FollowingTrader>>("/api/v2/copy/mix-follower/following-traders", requiresAuth: true, cancellationToken);
    }
}
