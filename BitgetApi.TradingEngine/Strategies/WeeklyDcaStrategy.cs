using BitgetApi.TradingEngine.Models;

namespace BitgetApi.TradingEngine.Strategies;

/// <summary>
/// Weekly Dollar Cost Averaging Strategy
/// Buys on a weekly schedule regardless of price
/// </summary>
public class WeeklyDcaStrategy : IStrategy
{
    public string Name => "Weekly_DCA";
    public string Market => "Spot";
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object> Parameters { get; set; }

    private DayOfWeek _buyDay = DayOfWeek.Monday;
    private int _buyHour = 10; // 10 AM UTC
    private decimal _buyAmountUsd = 100m;
    private DateTime? _lastBuyDate = null;

    public WeeklyDcaStrategy(Dictionary<string, object>? parameters = null)
    {
        Parameters = parameters ?? new Dictionary<string, object>();
        IsEnabled = true;
        
        if (Parameters.ContainsKey("buy_day"))
            _buyDay = (DayOfWeek)Convert.ToInt32(Parameters["buy_day"]);
        if (Parameters.ContainsKey("buy_hour"))
            _buyHour = Convert.ToInt32(Parameters["buy_hour"]);
        if (Parameters.ContainsKey("buy_amount_usd"))
            _buyAmountUsd = Convert.ToDecimal(Parameters["buy_amount_usd"]);
    }

    public async Task<Signal?> GenerateSignalAsync(string symbol, List<Candle> candles)
    {
        Console.WriteLine($"🔍 [Weekly_DCA] Checking {symbol}");
        
        try
        {
            var now = DateTime.UtcNow;
            var currentPrice = candles.Last().Close;

            // Check if it's the right day and hour
            if (now.DayOfWeek != _buyDay || now.Hour != _buyHour)
            {
                Console.WriteLine($"ℹ️ [Weekly_DCA] Not buy time (now: {now.DayOfWeek} {now.Hour}:00, target: {_buyDay} {_buyHour}:00)");
                return await Task.FromResult<Signal?>(null);
            }

            // Check if already bought this week
            if (_lastBuyDate.HasValue && (now - _lastBuyDate.Value).TotalDays < 6)
            {
                Console.WriteLine($"ℹ️ [Weekly_DCA] Already bought this week ({_lastBuyDate.Value:yyyy-MM-dd})");
                return await Task.FromResult<Signal?>(null);
            }

            // Generate DCA buy signal
            _lastBuyDate = now;
            
            var stopLoss = currentPrice * 0.90m; // 10% stop loss
            var takeProfit = currentPrice * 1.20m; // 20% take profit (long-term hold)
            
            Console.WriteLine($"✅ [Weekly_DCA] DCA buy signal: ${_buyAmountUsd} at ${currentPrice:F2}");

            return await Task.FromResult(new Signal
            {
                Symbol = symbol,
                Strategy = Name,
                Type = SignalType.LONG,
                EntryPrice = currentPrice,
                StopLoss = stopLoss,
                TakeProfit = takeProfit,
                Confidence = 80, // DCA is consistent strategy
                Reason = $"Weekly DCA buy on {_buyDay}, ${_buyAmountUsd} investment",
                Timestamp = now
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [Weekly_DCA] ERROR: {ex.Message}");
            throw;
        }
    }

    public void UpdateParameters(Dictionary<string, object> newParameters)
    {
        Parameters = newParameters;
        
        if (Parameters.ContainsKey("buy_day"))
            _buyDay = (DayOfWeek)Convert.ToInt32(Parameters["buy_day"]);
        if (Parameters.ContainsKey("buy_hour"))
            _buyHour = Convert.ToInt32(Parameters["buy_hour"]);
        if (Parameters.ContainsKey("buy_amount_usd"))
            _buyAmountUsd = Convert.ToDecimal(Parameters["buy_amount_usd"]);
    }
}
