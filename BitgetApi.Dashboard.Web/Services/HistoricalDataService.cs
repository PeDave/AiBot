using BitgetApi.Dashboard.Web.Data;
using BitgetApi.Dashboard.Web.Data.Models;
using BitgetApi.Dashboard.Web.Data.Repositories;

namespace BitgetApi.Dashboard.Web.Services;

public class HistoricalDataService
{
    private readonly BitgetMarketDataService _marketDataService;
    private readonly ICandleRepository _candleRepository;
    private const int DefaultMaxCandles = 5000;
    private const int DailyMaxCandles = 730; // 2 years
    private const int BitgetApiBatchLimit = 200; // Bitget API limit
    
    private readonly Dictionary<string, int> _maxCandlesByInterval = new()
    {
        { "15m", DefaultMaxCandles },
        { "1H", DefaultMaxCandles },
        { "4H", DefaultMaxCandles },
        { "1D", DailyMaxCandles }
    };

    public HistoricalDataService(
        BitgetMarketDataService marketDataService,
        ICandleRepository candleRepository)
    {
        _marketDataService = marketDataService;
        _candleRepository = candleRepository;
    }

    public async Task<List<CandleData>> GetOrFetchCandlesAsync(
    string symbol,
    string interval,
    string productType = "SPOT")
    {
        var maxCandles = _maxCandlesByInterval.GetValueOrDefault(interval, 5000);
        var dbCandles = await _candleRepository.GetCandlesAsync(symbol, interval, maxCount: maxCandles);

        // ✅ FIX: Ha nincs adat a DB-ben, azonnal hívjuk az API-t
        if (!dbCandles.Any())
        {
            Console.WriteLine($"No candles in DB for {symbol} {interval}, fetching from API...");
            return await FetchAndStoreCandlesAsync(symbol, interval, productType);
        }

        // Ha van adat, ellenőrizzük hogy friss-e
        var latestCandle = dbCandles.MaxBy(c => c.Timestamp);
        var latestTime = DateTimeOffset.FromUnixTimeMilliseconds(latestCandle!.Timestamp).UtcDateTime;

        // Ha az utolsó candle régebbi mint az interval, frissítsük
        if (DateTime.UtcNow - latestTime > GetIntervalTimeSpan(interval))
        {
            Console.WriteLine($"Candles for {symbol} {interval} are outdated, fetching fresh data...");
            return await FetchAndStoreCandlesAsync(symbol, interval, productType);
        }

        Console.WriteLine($"Returning {dbCandles.Count} cached candles for {symbol} {interval}");
        return dbCandles;
    }

    public async Task<List<CandleData>> FetchAndStoreCandlesAsync(
        string symbol, 
        string interval, 
        string productType = "SPOT")
    {
        try
        {
            Console.WriteLine($"📊 Fetching candles for {symbol} ({productType}) - Interval: {interval}");
            
            var maxCandles = _maxCandlesByInterval.GetValueOrDefault(interval, DefaultMaxCandles);
            var totalFetched = 0;
            var allCandles = new List<CandleData>();

            while (totalFetched < maxCandles)
            {
                var candles = await _marketDataService.GetHistoricalCandlesAsync(
                    symbol, 
                    interval, 
                    productType,
                    Math.Min(BitgetApiBatchLimit, maxCandles - totalFetched));

                if (!candles.Any())
                {
                    Console.WriteLine($"⚠️ No more candles available. Total fetched: {totalFetched}");
                    break;
                }

                Console.WriteLine($"✅ Fetched {candles.Count} candles from API");

                var candleModels = candles.Select(c => new CandleData
                {
                    Symbol = symbol,
                    Interval = interval,
                    Timestamp = c.Timestamp,
                    Open = c.Open,
                    High = c.High,
                    Low = c.Low,
                    Close = c.Close,
                    Volume = c.Volume,
                    QuoteVolume = c.QuoteVolume
                }).ToList();

                await _candleRepository.SaveCandlesAsync(candleModels);
                allCandles.AddRange(candleModels);
                totalFetched += candles.Count;

                if (candles.Count < BitgetApiBatchLimit) break; // No more data available
            }

            // Cleanup old candles
            await _candleRepository.CleanupOldCandlesAsync(symbol, interval, maxCandles);

            Console.WriteLine($"✅ Total candles stored: {allCandles.Count} for {symbol} {interval}");
            return allCandles;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error in FetchAndStoreCandlesAsync: {ex.Message}");
            Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
            return new List<CandleData>();
        }
    }

    private TimeSpan GetIntervalTimeSpan(string interval) => interval switch
    {
        "15m" => TimeSpan.FromMinutes(15),
        "1H" => TimeSpan.FromHours(1),
        "4H" => TimeSpan.FromHours(4),
        "1D" => TimeSpan.FromDays(1),
        _ => TimeSpan.FromHours(1)
    };
}
