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
        var scored = tickers.Select(t => new SymbolScore
        {
            Symbol = t.Symbol,
            Price = decimal.Parse(t.LastPrice),
            Volume = t.QuoteVolume,
            PriceChange = t.ChangeUtc,
            High24h = decimal.Parse(t.High24h),
            Low24h = decimal.Parse(t.Low24h),
            Score = CalculateScore(t)
        })
        .OrderByDescending(s => s.Score)
        .Take(count)
        .ToList();
        
        Console.WriteLine($"✅ Top {scored.Count} symbols:");
        foreach (var item in scored.Take(5))
        {
            Console.WriteLine($"   {item.Symbol}: Score={item.Score:F2}, Vol=${item.Volume/1_000_000:F2}M, Change={item.PriceChange:F2}%");
        }
        
        return scored;
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
