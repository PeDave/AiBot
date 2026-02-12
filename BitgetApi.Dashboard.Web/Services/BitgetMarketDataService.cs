using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace BitgetApi.Dashboard.Web.Services;

public class BitgetMarketDataService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://api.bitget.com";

    public BitgetMarketDataService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<List<SymbolData>> GetSpotSymbolsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<BitgetApiResponse<List<SymbolData>>>(
                "/api/v2/spot/public/symbols");
            return response?.Data ?? new List<SymbolData>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching spot symbols: {ex.Message}");
            return new List<SymbolData>();
        }
    }

    public async Task<List<SymbolData>> GetFuturesSymbolsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<BitgetApiResponse<List<SymbolData>>>(
                "/api/v2/mix/market/contracts?productType=USDT-FUTURES");
            return response?.Data ?? new List<SymbolData>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching futures symbols: {ex.Message}");
            return new List<SymbolData>();
        }
    }

    public async Task<List<CandleResponse>> GetHistoricalCandlesAsync(
        string symbol, 
        string interval, 
        string productType = "SPOT",
        int limit = 200)
    {
        try
        {
            var endpoint = productType == "SPOT"
                ? $"/api/v2/spot/market/candles?symbol={symbol}&granularity={interval}&limit={limit}"
                : $"/api/v2/mix/market/candles?symbol={symbol}&granularity={interval}&limit={limit}&productType={productType}";

            var response = await _httpClient.GetFromJsonAsync<BitgetApiResponse<List<List<string>>>>(endpoint);
            
            if (response?.Data == null) return new List<CandleResponse>();

            return response.Data.Select(c => new CandleResponse
            {
                Timestamp = long.Parse(c[0]),
                Open = decimal.Parse(c[1]),
                High = decimal.Parse(c[2]),
                Low = decimal.Parse(c[3]),
                Close = decimal.Parse(c[4]),
                Volume = decimal.Parse(c[5]),
                QuoteVolume = c.Count > 6 ? decimal.Parse(c[6]) : 0
            }).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching candles: {ex.Message}");
            return new List<CandleResponse>();
        }
    }
}

public class BitgetApiResponse<T>
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = "";
    
    [JsonPropertyName("msg")]
    public string Message { get; set; } = "";
    
    [JsonPropertyName("data")]
    public T? Data { get; set; }
}

public class SymbolData
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = "";
    
    [JsonPropertyName("baseCoin")]
    public string BaseCoin { get; set; } = "";
    
    [JsonPropertyName("quoteCoin")]
    public string QuoteCoin { get; set; } = "";
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = "";
}

public class CandleResponse
{
    public long Timestamp { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public decimal QuoteVolume { get; set; }
}
