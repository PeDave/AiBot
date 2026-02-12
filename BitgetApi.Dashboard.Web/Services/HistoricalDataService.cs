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
        // Try to get from DB first
        var maxCandles = _maxCandlesByInterval.GetValueOrDefault(interval, 5000);
        var dbCandles = await _candleRepository.GetCandlesAsync(symbol, interval, maxCount: maxCandles);

        // If we have recent data, return it
        if (dbCandles.Any())
        {
            var latestCandle = dbCandles.MaxBy(c => c.Timestamp);
            var latestTime = DateTimeOffset.FromUnixTimeMilliseconds(latestCandle!.Timestamp).UtcDateTime;
            
            // If latest candle is less than interval old, return cached data
            if (DateTime.UtcNow - latestTime < GetIntervalTimeSpan(interval))
            {
                return dbCandles;
            }
        }

        // Fetch fresh data from API
        var freshCandles = await FetchAndStoreCandlesAsync(symbol, interval, productType);
        return freshCandles;
    }

    public async Task<List<CandleData>> FetchAndStoreCandlesAsync(
        string symbol, 
        string interval, 
        string productType = "SPOT")
    {
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

            if (!candles.Any()) break;

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

        return allCandles;
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
