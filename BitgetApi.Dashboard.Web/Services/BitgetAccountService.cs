using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BitgetApi.Config;
using Microsoft.Extensions.Options;

namespace BitgetApi.Dashboard.Web.Services;

public class BitgetAccountService
{
    private readonly HttpClient _httpClient;
    private readonly BitgetApiConfig _config;

    public BitgetAccountService(HttpClient httpClient, IOptions<BitgetApiConfig> config)
    {
        _httpClient = httpClient;
        _config = config.Value;
        _httpClient.BaseAddress = new Uri(_config.BaseUrl);
    }

    public async Task<SpotAccountResponse?> GetSpotAccountAsync()
    {
        var endpoint = "/api/v2/spot/account/assets";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        AddAuthHeaders(request, "GET", endpoint, "", timestamp);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<SpotAccountResponse>();
    }

    public async Task<FuturesAccountResponse?> GetFuturesAccountAsync()
    {
        var endpoint = "/api/v2/mix/account/accounts?productType=USDT-FUTURES";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        AddAuthHeaders(request, "GET", endpoint, "", timestamp);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<FuturesAccountResponse>();
    }

    public async Task<EarnAccountResponse?> GetEarnAccountAsync()
    {
        var endpoint = "/api/v2/earn/account/assets";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        AddAuthHeaders(request, "GET", endpoint, "", timestamp);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadFromJsonAsync<EarnAccountResponse>();
    }

    public async Task<FuturesBotAccountResponse?> GetFuturesBotAccountAsync()
    {
        try
        {
            var endpoint = "/api/v2/copy/mix-trader/account-assets?productType=USDT-FUTURES";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            AddAuthHeaders(request, "GET", endpoint, "", timestamp);
            
            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"⚠️ Futures bot account not accessible (status: {response.StatusCode})");
                return null;
            }
            
            return await response.Content.ReadFromJsonAsync<FuturesBotAccountResponse>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Error fetching futures bot account: {ex.Message}");
            return null;
        }
    }

    private void AddAuthHeaders(HttpRequestMessage request, string method, string endpoint, string body, string timestamp)
    {
        var sign = GenerateSignature(timestamp, method, endpoint, body);
        
        request.Headers.Add("ACCESS-KEY", _config.ApiKey);
        request.Headers.Add("ACCESS-SIGN", sign);
        request.Headers.Add("ACCESS-TIMESTAMP", timestamp);
        request.Headers.Add("ACCESS-PASSPHRASE", _config.Passphrase);
    }

    private string GenerateSignature(string timestamp, string method, string endpoint, string body)
    {
        var message = timestamp + method.ToUpper() + endpoint + body;
        var keyBytes = Encoding.UTF8.GetBytes(_config.SecretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);
        
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(messageBytes);
        return Convert.ToBase64String(hash);
    }
}

// Response models
public class SpotAccountResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = "";
    
    [JsonPropertyName("msg")]
    public string Message { get; set; } = "";
    
    [JsonPropertyName("data")]
    public List<SpotAsset> Data { get; set; } = new();
}

public class SpotAsset
{
    [JsonPropertyName("coin")]
    public string Coin { get; set; } = "";
    
    [JsonPropertyName("available")]
    public string Available { get; set; } = "";
    
    [JsonPropertyName("frozen")]
    public string Frozen { get; set; } = "";
    
    [JsonPropertyName("usdtValue")]
    public string UsdtValue { get; set; } = "";
}

public class FuturesAccountResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = "";
    
    [JsonPropertyName("data")]
    public List<FuturesAccount> Data { get; set; } = new();
}

public class FuturesAccount
{
    [JsonPropertyName("marginCoin")]
    public string MarginCoin { get; set; } = "";
    
    [JsonPropertyName("available")]
    public string Available { get; set; } = "";
    
    [JsonPropertyName("frozen")]
    public string Frozen { get; set; } = "";
    
    [JsonPropertyName("equity")]
    public string Equity { get; set; } = "";
    
    [JsonPropertyName("usdtEquity")]
    public string UsdtEquity { get; set; } = "";
}

public class EarnAccountResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = "";
    
    [JsonPropertyName("data")]
    public List<EarnAsset> Data { get; set; } = new();
}

public class EarnAsset
{
    [JsonPropertyName("coin")]
    public string Coin { get; set; } = "";
    
    [JsonPropertyName("amount")]
    public string Amount { get; set; } = "";
    
    [JsonPropertyName("usdtValue")]
    public string UsdtValue { get; set; } = "";
}

public class FuturesBotAccountResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = "";
    
    [JsonPropertyName("data")]
    public List<FuturesBotAsset> Data { get; set; } = new();
}

public class FuturesBotAsset
{
    [JsonPropertyName("coin")]
    public string Coin { get; set; } = "";
    
    [JsonPropertyName("available")]
    public string Available { get; set; } = "";
    
    [JsonPropertyName("frozen")]
    public string Frozen { get; set; } = "";
    
    [JsonPropertyName("equity")]
    public string Equity { get; set; } = "";
}
