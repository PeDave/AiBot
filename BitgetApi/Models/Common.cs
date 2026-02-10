using System.Text.Json.Serialization;

namespace BitgetApi.Models;

/// <summary>
/// Generic wrapper for Bitget API responses
/// </summary>
/// <typeparam name="T">Response data type</typeparam>
public class BitgetResponse<T>
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("msg")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("requestTime")]
    public long RequestTime { get; set; }

    [JsonPropertyName("data")]
    public T? Data { get; set; }

    public bool IsSuccess => Code == "00000";
}

/// <summary>
/// API credentials for authentication
/// </summary>
public class BitgetCredentials
{
    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Passphrase { get; set; } = string.Empty;
}

/// <summary>
/// Order side enumeration
/// </summary>
public enum OrderSide
{
    Buy,
    Sell
}

/// <summary>
/// Order type enumeration
/// </summary>
public enum OrderType
{
    Limit,
    Market
}

/// <summary>
/// Time in force enumeration
/// </summary>
public enum TimeInForce
{
    GTC,        // Good Till Cancel
    IOC,        // Immediate or Cancel
    FOK,        // Fill or Kill
    POST_ONLY   // Post Only
}

/// <summary>
/// Product type for futures
/// </summary>
public enum ProductType
{
    [JsonPropertyName("USDT-FUTURES")]
    UsdtFutures,
    
    [JsonPropertyName("USDC-FUTURES")]
    UsdcFutures,
    
    [JsonPropertyName("COIN-FUTURES")]
    CoinFutures,
    
    [JsonPropertyName("SUSDT-FUTURES")]
    SusdtFutures,
    
    [JsonPropertyName("SUSDC-FUTURES")]
    SusdcFutures,
    
    [JsonPropertyName("SCOIN-FUTURES")]
    ScoinFutures
}

/// <summary>
/// Margin mode enumeration
/// </summary>
public enum MarginMode
{
    [JsonPropertyName("crossed")]
    Crossed,
    
    [JsonPropertyName("isolated")]
    Isolated
}

/// <summary>
/// Position mode enumeration
/// </summary>
public enum PositionMode
{
    [JsonPropertyName("one_way_mode")]
    OneWay,
    
    [JsonPropertyName("hedge_mode")]
    Hedge
}
