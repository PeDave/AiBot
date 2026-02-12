using BitgetApi.Dashboard.Web.Data.Models;

namespace BitgetApi.Dashboard.Web.Data.Repositories;

public interface ICandleRepository
{
    Task<List<CandleData>> GetCandlesAsync(string symbol, string interval, DateTime? fromTime = null, int maxCount = 5000);
    Task SaveCandlesAsync(List<CandleData> candles);
    Task CleanupOldCandlesAsync(string symbol, string interval, int maxCandles);
}
