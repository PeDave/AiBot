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
        // Try V2 first
        var v2Result = await GetSpotAccountV2Async();
        if (v2Result?.GetAssets().Any() == true)
            return v2Result;
        
        if (_config.EnableDebugLogging)
        {
            Console.WriteLine("⚠️ V2 API returned no assets, trying V1...");
        }
        
        // Fallback to V1
        return await GetSpotAccountV1Async();
    }

    private async Task<SpotAccountResponse?> GetSpotAccountV2Async()
    {
        var endpoint = "/api/v2/spot/account/assets";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        
        if (_config.EnableDebugLogging)
        {
            Console.WriteLine($"🔍 Calling Spot Account API (V2): {_httpClient.BaseAddress}{endpoint}");
        }
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        AddAuthHeaders(request, "GET", endpoint, "", timestamp);
        
        var response = await _httpClient.SendAsync(request);
        
        // Log raw response body
        var responseBody = await response.Content.ReadAsStringAsync();
        
        if (_config.EnableDebugLogging)
        {
            Console.WriteLine($"📥 Spot Account Response (V2) ({response.StatusCode}):");
            Console.WriteLine(responseBody);
        }
        
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"❌ Spot API (V2) failed with status {response.StatusCode}");
            return null;
        }
        
        // Deserialize and log parsed data
        var result = JsonSerializer.Deserialize<SpotAccountResponse>(
            responseBody, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        
        if (_config.EnableDebugLogging)
        {
            Console.WriteLine($"📊 Spot Response Code: {result?.Code}, Message: {result?.Message}");
            var assets = result?.GetAssets() ?? new List<SpotAsset>();
            Console.WriteLine($"📊 Spot Assets Count: {assets.Count}");
            
            if (assets.Any())
            {
                foreach (var asset in assets)
                {
                    Console.WriteLine($"   ✅ {asset.Coin}: Available={asset.Available}, Frozen={asset.Frozen}, USDT={asset.UsdtValue}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ No assets returned from Spot API (V2)");
            }
        }
        
        return result;
    }

    private async Task<SpotAccountResponse?> GetSpotAccountV1Async()
    {
        var endpoint = "/api/spot/v1/account/assets";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        
        if (_config.EnableDebugLogging)
        {
            Console.WriteLine($"🔍 Trying V1 Spot Account API: {endpoint}");
        }
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        AddAuthHeaders(request, "GET", endpoint, "", timestamp);
        
        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        
        if (_config.EnableDebugLogging)
        {
            Console.WriteLine($"📥 V1 Spot Response ({response.StatusCode}):");
            Console.WriteLine(responseBody);
        }
        
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"❌ Spot API (V1) failed with status {response.StatusCode}");
            return null;
        }
        
        var result = JsonSerializer.Deserialize<SpotAccountResponse>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        
        if (_config.EnableDebugLogging && result != null)
        {
            var assets = result.GetAssets();
            Console.WriteLine($"📊 V1 Spot Assets Count: {assets.Count}");
        }
        
        return result;
    }

    public async Task<FuturesAccountResponse?> GetFuturesAccountAsync()
    {
        var endpoint = "/api/v2/mix/account/accounts?productType=USDT-FUTURES";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        
        if (_config.EnableDebugLogging)
        {
            Console.WriteLine($"🔍 Calling Futures Account API: {_httpClient.BaseAddress}{endpoint}");
        }
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        AddAuthHeaders(request, "GET", endpoint, "", timestamp);
        
        var response = await _httpClient.SendAsync(request);
        
        // Log raw response body
        var responseBody = await response.Content.ReadAsStringAsync();
        
        if (_config.EnableDebugLogging)
        {
            Console.WriteLine($"📥 Futures Account Response ({response.StatusCode}):");
            Console.WriteLine(responseBody);
        }
        
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"❌ Futures API failed with status {response.StatusCode}");
            return null;
        }
        
        // Deserialize and log parsed data
        var result = JsonSerializer.Deserialize<FuturesAccountResponse>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        
        if (_config.EnableDebugLogging)
        {
            Console.WriteLine($"📊 Futures Response Code: {result?.Code}");
            var accounts = result?.GetAccounts() ?? new List<FuturesAccount>();
            Console.WriteLine($"📊 Futures Accounts Count: {accounts.Count}");
            
            if (accounts.Any())
            {
                foreach (var account in accounts)
                {
                    Console.WriteLine($"   ✅ {account.MarginCoin}: Available={account.Available}, Frozen={account.Frozen}, Equity={account.UsdtEquity}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ No accounts returned from Futures API");
            }
        }
        
        return result;
    }

    public async Task<EarnAccountResponse?> GetEarnAccountAsync()
    {
        var endpoint = "/api/v2/earn/account/assets";
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        
        if (_config.EnableDebugLogging)
        {
            Console.WriteLine($"🔍 Calling Earn Account API: {_httpClient.BaseAddress}{endpoint}");
        }
        
        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
        AddAuthHeaders(request, "GET", endpoint, "", timestamp);
        
        var response = await _httpClient.SendAsync(request);
        
        // Log raw response body
        var responseBody = await response.Content.ReadAsStringAsync();
        
        if (_config.EnableDebugLogging)
        {
            Console.WriteLine($"📥 Earn Account Response ({response.StatusCode}):");
            Console.WriteLine(responseBody);
        }
        
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"❌ Earn API failed with status {response.StatusCode}");
            return null;
        }
        
        // Deserialize and log parsed data
        var result = JsonSerializer.Deserialize<EarnAccountResponse>(
            responseBody,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
        
        if (_config.EnableDebugLogging)
        {
            Console.WriteLine($"📊 Earn Response Code: {result?.Code}");
            var assets = result?.GetAssets() ?? new List<EarnAsset>();
            Console.WriteLine($"📊 Earn Assets Count: {assets.Count}");
            
            if (assets.Any())
            {
                foreach (var asset in assets)
                {
                    Console.WriteLine($"   ✅ {asset.Coin}: Amount={asset.Amount}, USDT={asset.UsdtValue}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ No assets returned from Earn API");
            }
        }
        
        return result;
    }

    public async Task<FuturesBotAccountResponse?> GetFuturesBotAccountAsync()
    {
        try
        {
            var endpoint = "/api/v2/copy/mix-trader/account-assets?productType=USDT-FUTURES";
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            
            if (_config.EnableDebugLogging)
            {
                Console.WriteLine($"🔍 Calling Futures Bot Account API: {_httpClient.BaseAddress}{endpoint}");
            }
            
            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            AddAuthHeaders(request, "GET", endpoint, "", timestamp);
            
            var response = await _httpClient.SendAsync(request);
            
            // Log raw response body
            var responseBody = await response.Content.ReadAsStringAsync();
            
            if (_config.EnableDebugLogging)
            {
                Console.WriteLine($"📥 Futures Bot Account Response ({response.StatusCode}):");
                Console.WriteLine(responseBody);
            }
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"⚠️ Futures bot account not accessible (status: {response.StatusCode})");
                return null;
            }
            
            // Deserialize and log parsed data
            var result = JsonSerializer.Deserialize<FuturesBotAccountResponse>(
                responseBody,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
            
            if (_config.EnableDebugLogging)
            {
                Console.WriteLine($"📊 Futures Bot Response Code: {result?.Code}");
                var assets = result?.GetAssets() ?? new List<FuturesBotAsset>();
                Console.WriteLine($"📊 Futures Bot Assets Count: {assets.Count}");
                
                if (assets.Any())
                {
                    foreach (var asset in assets)
                    {
                        Console.WriteLine($"   ✅ {asset.Coin}: Available={asset.Available}, Frozen={asset.Frozen}, Equity={asset.Equity}");
                    }
                }
                else
                {
                    Console.WriteLine($"⚠️ No assets returned from Futures Bot API");
                }
            }
            
            return result;
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
    public List<SpotAsset>? Data { get; set; }
    
    // Alternative: nested list structure
    [JsonPropertyName("data")]
    public SpotAccountDataWrapper? DataWrapper { get; set; }
    
    // Helper to get assets from either structure
    public List<SpotAsset> GetAssets()
    {
        if (Data != null && Data.Any())
            return Data;
        
        if (DataWrapper?.List != null && DataWrapper.List.Any())
            return DataWrapper.List;
        
        return new List<SpotAsset>();
    }
}

public class SpotAccountDataWrapper
{
    [JsonPropertyName("list")]
    public List<SpotAsset> List { get; set; } = new();
}

public class SpotAsset
{
    [JsonPropertyName("coin")]
    public string Coin { get; set; } = "";
    
    [JsonPropertyName("available")]
    public string Available { get; set; } = "";
    
    [JsonPropertyName("frozen")]
    public string Frozen { get; set; } = "";
    
    [JsonPropertyName("locked")]
    public string? Locked { get; set; } // Alternative field name
    
    [JsonPropertyName("usdtValue")]
    public string UsdtValue { get; set; } = "";
}

public class FuturesAccountResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = "";
    
    [JsonPropertyName("data")]
    public List<FuturesAccount>? Data { get; set; }
    
    // Alternative: nested list structure
    [JsonPropertyName("data")]
    public FuturesAccountDataWrapper? DataWrapper { get; set; }
    
    // Helper to get accounts from either structure
    public List<FuturesAccount> GetAccounts()
    {
        if (Data != null && Data.Any())
            return Data;
        
        if (DataWrapper?.List != null && DataWrapper.List.Any())
            return DataWrapper.List;
        
        return new List<FuturesAccount>();
    }
}

public class FuturesAccountDataWrapper
{
    [JsonPropertyName("list")]
    public List<FuturesAccount> List { get; set; } = new();
}

public class FuturesAccount
{
    [JsonPropertyName("marginCoin")]
    public string MarginCoin { get; set; } = "";
    
    [JsonPropertyName("available")]
    public string Available { get; set; } = "";
    
    [JsonPropertyName("frozen")]
    public string Frozen { get; set; } = "";
    
    [JsonPropertyName("locked")]
    public string? Locked { get; set; } // Alternative field name
    
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
    public List<EarnAsset>? Data { get; set; }
    
    // Alternative: nested list structure
    [JsonPropertyName("data")]
    public EarnAccountDataWrapper? DataWrapper { get; set; }
    
    // Helper to get assets from either structure
    public List<EarnAsset> GetAssets()
    {
        if (Data != null && Data.Any())
            return Data;
        
        if (DataWrapper?.List != null && DataWrapper.List.Any())
            return DataWrapper.List;
        
        return new List<EarnAsset>();
    }
}

public class EarnAccountDataWrapper
{
    [JsonPropertyName("list")]
    public List<EarnAsset> List { get; set; } = new();
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
    public List<FuturesBotAsset>? Data { get; set; }
    
    // Alternative: nested list structure
    [JsonPropertyName("data")]
    public FuturesBotAccountDataWrapper? DataWrapper { get; set; }
    
    // Helper to get assets from either structure
    public List<FuturesBotAsset> GetAssets()
    {
        if (Data != null && Data.Any())
            return Data;
        
        if (DataWrapper?.List != null && DataWrapper.List.Any())
            return DataWrapper.List;
        
        return new List<FuturesBotAsset>();
    }
}

public class FuturesBotAccountDataWrapper
{
    [JsonPropertyName("list")]
    public List<FuturesBotAsset> List { get; set; } = new();
}

public class FuturesBotAsset
{
    [JsonPropertyName("coin")]
    public string Coin { get; set; } = "";
    
    [JsonPropertyName("available")]
    public string Available { get; set; } = "";
    
    [JsonPropertyName("frozen")]
    public string Frozen { get; set; } = "";
    
    [JsonPropertyName("locked")]
    public string? Locked { get; set; } // Alternative field name
    
    [JsonPropertyName("equity")]
    public string Equity { get; set; } = "";
}
