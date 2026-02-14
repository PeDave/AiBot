using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BitgetApi.TradingEngine.Services;

public class AltcoinScannerService
{
    private readonly ILogger<AltcoinScannerService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly BitgetMarketDataService _marketDataService;

    public AltcoinScannerService(
        ILogger<AltcoinScannerService> logger,
        IConfiguration configuration,
        HttpClient httpClient,
        BitgetMarketDataService marketDataService)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
        _marketDataService = marketDataService;
    }

    public async Task<List<string>> ScanAndSelectTopCoinsAsync()
    {
        try
        {
            _logger.LogInformation("🔍 Starting altcoin scan...");

            // Get all tickers from Bitget
            var tickers = await FetchAllTickersAsync();
            
            if (tickers == null || tickers.Count == 0)
            {
                _logger.LogWarning("⚠️ No tickers received from Bitget API");
                return GetDefaultWatchlist();
            }

            // Filter USDT pairs only
            var usdtPairs = tickers
                .Where(t => t.Symbol.EndsWith("USDT") && !t.Symbol.StartsWith("USDT"))
                .ToList();

            _logger.LogInformation("📊 Analyzing {Count} USDT pairs", usdtPairs.Count);

            // Score each coin
            var scoredCoins = new List<AltcoinScore>();
            
            foreach (var ticker in usdtPairs)
            {
                var score = CalculateScore(ticker);
                if (score != null && score.TotalScore > 0)
                {
                    scoredCoins.Add(score);
                }
            }

            // Sort by score and take top N
            var topCount = _configuration.GetValue<int>("AltcoinScanner:TopCoinCount", 6);
            var topCoins = scoredCoins
                .OrderByDescending(c => c.TotalScore)
                .Take(topCount)
                .Select(c => c.Symbol)
                .ToList();

            _logger.LogInformation("🪙 Top {Count} coins selected:", topCoins.Count);
            foreach (var coin in topCoins)
            {
                var score = scoredCoins.First(c => c.Symbol == coin);
                _logger.LogInformation("   • {Symbol}: {Score:F2} (Vol: {Volume:F0}, Momentum: {Momentum:F2}%)", 
                    coin, score.TotalScore, score.Volume24h, score.Momentum);
            }

            return topCoins;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error scanning altcoins");
            return GetDefaultWatchlist();
        }
    }

    private async Task<List<BitgetTicker>> FetchAllTickersAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("https://api.bitget.com/api/v2/spot/market/tickers");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<BitgetTickerResponse>(json);

            return result?.Data ?? new List<BitgetTicker>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Bitget tickers");
            return new List<BitgetTicker>();
        }
    }

    private AltcoinScore? CalculateScore(BitgetTicker ticker)
    {
        try
        {
            // Parse values
            if (!decimal.TryParse(ticker.Volume24h, out var volume24h)) return null;
            if (!decimal.TryParse(ticker.High24h, out var high24h)) return null;
            if (!decimal.TryParse(ticker.Low24h, out var low24h)) return null;
            if (!decimal.TryParse(ticker.LastPrice, out var lastPrice)) return null;
            if (!decimal.TryParse(ticker.PriceChange24h, out var priceChange24h)) return null;

            // Filters
            var minVolume = _configuration.GetValue<decimal>("AltcoinScanner:MinVolume24h", 10_000_000);
            if (volume24h < minVolume) return null;

            var minPriceChange = _configuration.GetValue<decimal>("AltcoinScanner:MinPriceChangePercent", 2.0m);
            if (Math.Abs(priceChange24h) < minPriceChange) return null;

            // Calculate metrics
            var volatility = high24h > 0 ? ((high24h - low24h) / high24h) * 100 : 0;
            var momentum = priceChange24h;

            // Weights
            var volumeWeight = _configuration.GetValue<decimal>("AltcoinScanner:Weights:Volume", 0.3m);
            var volatilityWeight = _configuration.GetValue<decimal>("AltcoinScanner:Weights:Volatility", 0.3m);
            var momentumWeight = _configuration.GetValue<decimal>("AltcoinScanner:Weights:Momentum", 0.4m);

            // Normalize and score
            var volumeScore = Math.Min((volume24h / 100_000_000m) * 100, 100); // Normalize to 100M = 100pts
            var volatilityScore = Math.Min(volatility * 10, 100); // 10% volatility = 100pts
            var momentumScore = Math.Min(Math.Abs(momentum) * 5, 100); // 20% change = 100pts

            var totalScore = (volumeScore * volumeWeight) + 
                           (volatilityScore * volatilityWeight) + 
                           (momentumScore * momentumWeight);

            return new AltcoinScore
            {
                Symbol = ticker.Symbol,
                Volume24h = volume24h,
                Volatility = volatility,
                Momentum = momentum,
                VolumeScore = volumeScore,
                VolatilityScore = volatilityScore,
                MomentumScore = momentumScore,
                TotalScore = totalScore
            };
        }
        catch
        {
            return null;
        }
    }

    private List<string> GetDefaultWatchlist()
    {
        return new List<string> { "BTCUSDT", "ETHUSDT", "SOLUSDT", "AVAXUSDT", "MATICUSDT", "ADAUSDT" };
    }
}

// Models
public class BitgetTickerResponse
{
    public List<BitgetTicker> Data { get; set; } = new();
}

public class BitgetTicker
{
    public string Symbol { get; set; } = "";
    public string High24h { get; set; } = "0";
    public string Low24h { get; set; } = "0";
    public string LastPrice { get; set; } = "0";
    public string Volume24h { get; set; } = "0";
    public string PriceChange24h { get; set; } = "0";
}

public class AltcoinScore
{
    public string Symbol { get; set; } = "";
    public decimal Volume24h { get; set; }
    public decimal Volatility { get; set; }
    public decimal Momentum { get; set; }
    public decimal VolumeScore { get; set; }
    public decimal VolatilityScore { get; set; }
    public decimal MomentumScore { get; set; }
    public decimal TotalScore { get; set; }
}
