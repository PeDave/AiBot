using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using BitgetApi.TradingEngine.Models;

namespace BitgetApi.TradingEngine.Services;

public class BitgetMarketDataService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Constructor - REQUIRED for AddHttpClient<T> dependency injection
    /// </summary>
    public BitgetMarketDataService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        // Ensure BaseAddress is set
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri("https://api.bitget.com");
        }

        // Add default headers
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    /// <summary>
    /// Get all USDT tickers from Bitget
    /// </summary>
    public async Task<List<TickerData>> GetAllTickersAsync(string quoteCurrency = "USDT")
    {
        try
        {
            var endpoint = "/api/v2/spot/market/tickers";

            Console.WriteLine($"📥 Fetching all {quoteCurrency} tickers from Bitget...");

            var response = await _httpClient.GetAsync(endpoint);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"❌ Failed to fetch tickers: {response.StatusCode}");
                return new List<TickerData>();
            }

            var result = await response.Content.ReadFromJsonAsync<TickerResponse>();

            if (result?.Data == null)
            {
                Console.WriteLine("⚠️ Tickers response has no data");
                return new List<TickerData>();
            }

            var filtered = result.Data
                .Where(t => t.Symbol.EndsWith(quoteCurrency))
                .ToList();

            Console.WriteLine($"✅ Fetched {filtered.Count} {quoteCurrency} pairs");

            return filtered;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error fetching tickers: {ex.Message}");
            return new List<TickerData>();
        }
    }

    /// <summary>
    /// Get ticker for a specific symbol
    /// </summary>
    public async Task<TickerData> GetTickerAsync(string symbol)
    {
        var tickers = await GetAllTickersAsync();
        var ticker = tickers.FirstOrDefault(t => t.Symbol == symbol);

        if (ticker == null)
            throw new Exception($"Ticker not found for {symbol}");

        return ticker;
    }

    /// <summary>
    /// Get candle data for a symbol
    /// </summary>
    public async Task<List<Candle>> GetCandlesAsync(string symbol, string timeframe = "1h", int limit = 200)
    {
        try
        {
            // Map timeframe to Bitget granularity
            var granularity = timeframe switch
            {
                "1m" => "1m",
                "5m" => "5m",
                "15m" => "15m",
                "30m" => "30m",
                "1h" => "1h",
                "4h" => "4h",
                "1d" => "1day",
                _ => "1h"
            };

            var endpoint = $"/api/v2/spot/market/candles?symbol={symbol}&granularity={granularity}&limit={limit}";

            Console.WriteLine($"🔍 Fetching candles: {_httpClient.BaseAddress}{endpoint}");

            var response = await _httpClient.GetAsync(endpoint);

            Console.WriteLine($"📥 Response status: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ Failed to fetch candles for {symbol}: {response.StatusCode}");
                Console.WriteLine($"   Error: {errorBody}");
                return new List<Candle>();
            }

            var result = await response.Content.ReadFromJsonAsync<CandleResponse>();

            if (result?.Data == null || !result.Data.Any())
            {
                Console.WriteLine($"⚠️ No candle data for {symbol}");
                return new List<Candle>();
            }

            Console.WriteLine($"✅ Received {result.Data.Count} candle arrays");

            var candles = result.Data.Select(c =>
            {
                // Safely parse candle data (array format: [timestamp, open, high, low, close, volume])
                if (!long.TryParse(c[0], out var timestamp) ||
                    !decimal.TryParse(c[1], CultureInfo.InvariantCulture, out var open) ||
                    !decimal.TryParse(c[2], CultureInfo.InvariantCulture, out var high) ||
                    !decimal.TryParse(c[3], CultureInfo.InvariantCulture, out var low) ||
                    !decimal.TryParse(c[4], CultureInfo.InvariantCulture, out var close) ||
                    !decimal.TryParse(c[5], CultureInfo.InvariantCulture, out var volume))
                {
                    Console.WriteLine($"⚠️ Failed to parse candle: [{string.Join(", ", c)}]");
                    return null;
                }

                return new Candle
                {
                    Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime,
                    Open = open,
                    High = high,
                    Low = low,
                    Close = close,
                    Volume = volume,
                    Symbol = symbol
                };
            })
            .Where(c => c != null)
            .Cast<Candle>()
            .OrderBy(c => c.Timestamp)
            .ToList();

            Console.WriteLine($"✅ Parsed {candles.Count} candles for {symbol}");

            return candles;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error fetching candles for {symbol}: {ex.Message}");
            Console.WriteLine($"   Stack: {ex.StackTrace}");
            return new List<Candle>();
        }
    }
}

// ════════════════════════════════════════════════════════
// MODELS
// ════════════════════════════════════════════════════════

public class TickerResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = "";

    [JsonPropertyName("data")]
    public List<TickerData> Data { get; set; } = new();
}

public class TickerData
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = "";

    [JsonPropertyName("lastPr")]
    public string LastPrice { get; set; } = "";

    [JsonPropertyName("quoteVolume")]
    public decimal QuoteVolume { get; set; }

    [JsonPropertyName("changeUtc")]
    public decimal ChangeUtc { get; set; }

    [JsonPropertyName("high24h")]
    public string High24h { get; set; } = "";

    [JsonPropertyName("low24h")]
    public string Low24h { get; set; } = "";
}

public class CandleResponse
{
    [JsonPropertyName("code")]
    public string Code { get; set; } = "";

    [JsonPropertyName("msg")]
    public string Message { get; set; } = "";

    [JsonPropertyName("data")]
    public List<List<string>> Data { get; set; } = new();  // Array of arrays!
}

/*public class Candle
{
    public DateTime Timestamp { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public string Symbol { get; set; } = "";
}*/