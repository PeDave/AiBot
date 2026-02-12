using Microsoft.EntityFrameworkCore;
using BitgetApi.Dashboard.Web.Data.Models;

namespace BitgetApi.Dashboard.Web.Data.Repositories;

public class CandleRepository : ICandleRepository
{
    private readonly MarketDataContext _context;

    public CandleRepository(MarketDataContext context)
    {
        _context = context;
    }

    public async Task<List<CandleData>> GetCandlesAsync(string symbol, string interval, DateTime? fromTime = null, int maxCount = 5000)
    {
        var query = _context.Candles
            .Where(c => c.Symbol == symbol && c.Interval == interval);

        if (fromTime.HasValue)
        {
            var fromTimestamp = new DateTimeOffset(fromTime.Value).ToUnixTimeMilliseconds();
            query = query.Where(c => c.Timestamp >= fromTimestamp);
        }

        return await query
            .OrderByDescending(c => c.Timestamp)
            .Take(maxCount)
            .OrderBy(c => c.Timestamp)
            .ToListAsync();
    }

    public async Task SaveCandlesAsync(List<CandleData> candles)
    {
        foreach (var candle in candles)
        {
            var existing = await _context.Candles
                .FirstOrDefaultAsync(c => c.Symbol == candle.Symbol 
                    && c.Interval == candle.Interval 
                    && c.Timestamp == candle.Timestamp);

            if (existing != null)
            {
                existing.Open = candle.Open;
                existing.High = candle.High;
                existing.Low = candle.Low;
                existing.Close = candle.Close;
                existing.Volume = candle.Volume;
                existing.QuoteVolume = candle.QuoteVolume;
            }
            else
            {
                await _context.Candles.AddAsync(candle);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task CleanupOldCandlesAsync(string symbol, string interval, int maxCandles)
    {
        var totalCount = await _context.Candles
            .CountAsync(c => c.Symbol == symbol && c.Interval == interval);

        if (totalCount > maxCandles)
        {
            var toDelete = totalCount - maxCandles;
            var oldCandles = await _context.Candles
                .Where(c => c.Symbol == symbol && c.Interval == interval)
                .OrderBy(c => c.Timestamp)
                .Take(toDelete)
                .ToListAsync();

            _context.Candles.RemoveRange(oldCandles);
            await _context.SaveChangesAsync();
        }
    }
}
