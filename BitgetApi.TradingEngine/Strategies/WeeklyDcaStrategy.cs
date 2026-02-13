using BitgetApi.TradingEngine.Models;
using BitgetApi.TradingEngine.Indicators;

namespace BitgetApi.TradingEngine.Strategies;

public class WeeklyDcaStrategy : IStrategy
{
    public string Name => "Weekly_DCA";
    public string Market => "Spot";
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object> Parameters { get; set; }

    private readonly EmaIndicator _emaIndicator;
    private readonly SmaIndicator _smaIndicator;
    private readonly LiquidityZoneDetector _liquidityDetector;
    private DateTime _lastWeeklyDca = DateTime.MinValue;
    private DateTime _lastDailyDca = DateTime.MinValue;

    public WeeklyDcaStrategy(Dictionary<string, object>? parameters = null)
    {
        Parameters = parameters ?? new Dictionary<string, object>
        {
            { "base_amount_usd", 10.0 },
            { "ema_period", 200 },
            { "sma_period", 200 },
            { "weekly_multiplier", 1.0 },
            { "daily_multiplier", 2.0 },
            { "liquidity_multiplier", 1.5 }
        };

        _emaIndicator = new EmaIndicator(GetParameter<int>("ema_period"));
        _smaIndicator = new SmaIndicator(GetParameter<int>("sma_period"));
        _liquidityDetector = new LiquidityZoneDetector();
    }

    public Task<Signal?> GenerateSignalAsync(string symbol, List<Candle> candles)
    {
        if (candles.Count < 250)
            return Task.FromResult<Signal?>(null);

        var currentPrice = candles.Last().Close;
        var ema200 = _emaIndicator.Calculate(candles);
        var sma200 = _smaIndicator.Calculate(candles);
        var liquidityZones = _liquidityDetector.DetectZones(candles);
        var nearLiquidity = _liquidityDetector.IsNearLiquidityZone(currentPrice, liquidityZones, 1.0);

        var now = DateTime.UtcNow;
        var baseAmount = GetParameter<double>("base_amount_usd");
        var weeklyMultiplier = GetParameter<double>("weekly_multiplier");
        var dailyMultiplier = GetParameter<double>("daily_multiplier");
        var liquidityMultiplier = GetParameter<double>("liquidity_multiplier");

        // Determine DCA type and amount
        var priceBelow200 = currentPrice < ema200 && currentPrice < sma200;
        
        // Check if price is below both 200 EMA and 200 SMA -> Daily DCA
        if (priceBelow200 && (now - _lastDailyDca).TotalHours >= 24)
        {
            _lastDailyDca = now;
            var dcaAmount = baseAmount * dailyMultiplier; // $20

            var signal = new Signal
            {
                Symbol = symbol,
                Strategy = Name,
                Type = SignalType.LONG,
                EntryPrice = currentPrice,
                StopLoss = 0, // No SL for DCA
                TakeProfit = 0, // No TP for DCA (long-term hold)
                Confidence = 90,
                Reason = $"Daily DCA - Price below 200 EMA ({ema200:F2}) and 200 SMA ({sma200:F2})"
            };

            signal.Metadata["dca_type"] = "Daily";
            signal.Metadata["dca_amount_usd"] = dcaAmount;
            signal.Metadata["price_below_200"] = true;

            return Task.FromResult<Signal?>(signal);
        }

        // Check if at liquidity zone -> Enhanced DCA
        if (nearLiquidity && (now - _lastWeeklyDca).TotalDays >= 7)
        {
            _lastWeeklyDca = now;
            var dcaAmount = baseAmount * liquidityMultiplier; // $15

            var signal = new Signal
            {
                Symbol = symbol,
                Strategy = Name,
                Type = SignalType.LONG,
                EntryPrice = currentPrice,
                StopLoss = 0,
                TakeProfit = 0,
                Confidence = 85,
                Reason = $"Liquidity Zone DCA - Price at support level"
            };

            signal.Metadata["dca_type"] = "LiquidityZone";
            signal.Metadata["dca_amount_usd"] = dcaAmount;
            signal.Metadata["near_liquidity"] = true;

            return Task.FromResult<Signal?>(signal);
        }

        // Default weekly DCA
        if ((now - _lastWeeklyDca).TotalDays >= 7)
        {
            _lastWeeklyDca = now;
            var dcaAmount = baseAmount * weeklyMultiplier; // $10

            var signal = new Signal
            {
                Symbol = symbol,
                Strategy = Name,
                Type = SignalType.LONG,
                EntryPrice = currentPrice,
                StopLoss = 0,
                TakeProfit = 0,
                Confidence = 75,
                Reason = $"Weekly DCA - Regular accumulation"
            };

            signal.Metadata["dca_type"] = "Weekly";
            signal.Metadata["dca_amount_usd"] = dcaAmount;

            return Task.FromResult<Signal?>(signal);
        }

        return Task.FromResult<Signal?>(null);
    }

    public void UpdateParameters(Dictionary<string, object> newParameters)
    {
        foreach (var param in newParameters)
        {
            Parameters[param.Key] = param.Value;
        }
    }

    private T GetParameter<T>(string key)
    {
        if (Parameters.TryGetValue(key, out var value))
        {
            if (value is T typedValue)
                return typedValue;
            
            return (T)Convert.ChangeType(value, typeof(T));
        }
        
        throw new KeyNotFoundException($"Parameter '{key}' not found");
    }
}
