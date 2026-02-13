namespace BitgetApi.TradingEngine.Services;

public class SymbolScanner
{
    private readonly BitgetMarketDataService _marketData;
    
    public SymbolScanner(BitgetMarketDataService marketData)
    {
        _marketData = marketData;
    }
    
    public async Task<List<SymbolScore>> ScanTopSymbolsAsync(int count = 10)
    {
        Console.WriteLine($"🔍 Scanning top {count} symbols from Bitget...");
        
        // Fetch all USDT tickers
        var tickers = await _marketData.GetAllTickersAsync("USDT");
        
        if (!tickers.Any())
        {
            Console.WriteLine("⚠️ No tickers returned from Bitget");
            return new List<SymbolScore>();
        }
        
        // Score each symbol
        var scored = new List<SymbolScore>();
        
        foreach (var t in tickers)
        {
            // Safely parse decimal values
            if (!decimal.TryParse(t.LastPrice, out var price) ||
                !decimal.TryParse(t.High24h, out var high24h) ||
                !decimal.TryParse(t.Low24h, out var low24h))
            {
                Console.WriteLine($"⚠️ Invalid price data for {t.Symbol}, skipping");
                continue;
            }
            
            scored.Add(new SymbolScore
            {
                Symbol = t.Symbol,
                Price = price,
                Volume = t.QuoteVolume,
                PriceChange = t.ChangeUtc,
                High24h = high24h,
                Low24h = low24h,
                Score = CalculateScore(t)
            });
        }
        
        var topScored = scored
            .OrderByDescending(s => s.Score)
            .Take(count)
            .ToList();
        
        Console.WriteLine($"✅ Top {topScored.Count} symbols:");
        foreach (var item in topScored.Take(5))
        {
            Console.WriteLine($"   {item.Symbol}: Score={item.Score:F2}, Vol=${item.Volume/1_000_000:F2}M, Change={item.PriceChange:F2}%");
        }
        
        return topScored;
    }
    
    private decimal CalculateScore(TickerData ticker)
    {
        // Volume weight: 40%
        var volumeScore = ticker.QuoteVolume / 10_000_000m;
        
        // Volatility weight: 60%
        var volatilityScore = Math.Abs(ticker.ChangeUtc) / 10m;
        
        return (volumeScore * 0.4m) + (volatilityScore * 0.6m);
    }
}

public class SymbolScore
{
    public string Symbol { get; set; } = "";
    public decimal Price { get; set; }
    public decimal Volume { get; set; }
    public decimal PriceChange { get; set; }
    public decimal High24h { get; set; }
    public decimal Low24h { get; set; }
    public decimal Score { get; set; }
}
