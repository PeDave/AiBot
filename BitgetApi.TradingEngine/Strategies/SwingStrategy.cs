using BitgetApi.TradingEngine.Models;

namespace BitgetApi.TradingEngine.Strategies;

/// <summary>
/// Swing Trading Strategy
/// Identifies swing highs/lows and trend reversals
/// </summary>
public class SwingStrategy : IStrategy
{
    public string Name => "Swing";
    public string Market => "Futures";
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object> Parameters { get; set; }

    private int _swingPeriod = 5;
    private decimal _minSwingPercent = 2.0m;

    public SwingStrategy(Dictionary<string, object>? parameters = null)
    {
        Parameters = parameters ?? new Dictionary<string, object>();
        IsEnabled = true;
        
        if (Parameters.ContainsKey("swing_period"))
            _swingPeriod = Convert.ToInt32(Parameters["swing_period"]);
        if (Parameters.ContainsKey("min_swing_percent"))
            _minSwingPercent = Convert.ToDecimal(Parameters["min_swing_percent"]);
    }

    public async Task<Signal?> GenerateSignalAsync(string symbol, List<Candle> candles)
    {
        Console.WriteLine($"🔍 [Swing] Analyzing {symbol} with {candles.Count} candles");
        
        try
        {
            if (candles.Count < _swingPeriod * 3)
            {
                Console.WriteLine($"⚠️ [Swing] Not enough candles (need {_swingPeriod * 3})");
                return await Task.FromResult<Signal?>(null);
            }

            var recentCandles = candles.TakeLast(_swingPeriod * 3).ToList();
            var currentPrice = candles.Last().Close;

            // Find swing low (potential buy)
            var swingLow = FindSwingLow(recentCandles, _swingPeriod);
            if (swingLow.HasValue)
            {
                var swingPercent = ((currentPrice - swingLow.Value) / swingLow.Value) * 100;
                
                if (swingPercent >= _minSwingPercent && currentPrice > swingLow.Value)
                {
                    var stopLoss = swingLow.Value * 0.98m;
                    var riskAmount = currentPrice - stopLoss;
                    var takeProfit = currentPrice + (riskAmount * 2m);
                    var confidence = Math.Min(85m, 50m + swingPercent * 2m);

                    Console.WriteLine($"✅ [Swing] Swing low detected at ${swingLow.Value:F2}, bounce {swingPercent:F2}%");

                    return await Task.FromResult(new Signal
                    {
                        Symbol = symbol,
                        Strategy = Name,
                        Type = SignalType.LONG,
                        EntryPrice = currentPrice,
                        StopLoss = stopLoss,
                        TakeProfit = takeProfit,
                        Confidence = (double)confidence,
                        Reason = $"Swing low reversal at ${swingLow.Value:F2}, bounce {swingPercent:F2}%",
                        Timestamp = DateTime.UtcNow
                    });
                }
            }

            // Find swing high (potential sell)
            var swingHigh = FindSwingHigh(recentCandles, _swingPeriod);
            if (swingHigh.HasValue)
            {
                var swingPercent = ((swingHigh.Value - currentPrice) / swingHigh.Value) * 100;
                
                if (swingPercent >= _minSwingPercent && currentPrice < swingHigh.Value)
                {
                    var stopLoss = swingHigh.Value * 1.02m;
                    var riskAmount = stopLoss - currentPrice;
                    var takeProfit = currentPrice - (riskAmount * 2m);
                    var confidence = Math.Min(85m, 50m + swingPercent * 2m);

                    Console.WriteLine($"✅ [Swing] Swing high detected at ${swingHigh.Value:F2}, drop {swingPercent:F2}%");

                    return await Task.FromResult(new Signal
                    {
                        Symbol = symbol,
                        Strategy = Name,
                        Type = SignalType.SHORT,
                        EntryPrice = currentPrice,
                        StopLoss = stopLoss,
                        TakeProfit = takeProfit,
                        Confidence = (double)confidence,
                        Reason = $"Swing high reversal at ${swingHigh.Value:F2}, drop {swingPercent:F2}%",
                        Timestamp = DateTime.UtcNow
                    });
                }
            }

            Console.WriteLine($"ℹ️ [Swing] No swing reversal detected");
            return await Task.FromResult<Signal?>(null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [Swing] ERROR: {ex.Message}");
            throw;
        }
    }

    private decimal? FindSwingLow(List<Candle> candles, int period)
    {
        if (candles.Count < period * 2 + 1) return null;

        var midIndex = candles.Count - period - 1;
        var midLow = candles[midIndex].Low;

        // Check if mid is lower than all candles in period before and after
        for (int i = midIndex - period; i < midIndex + period + 1; i++)
        {
            if (i != midIndex && candles[i].Low <= midLow)
                return null;
        }

        return midLow;
    }

    private decimal? FindSwingHigh(List<Candle> candles, int period)
    {
        if (candles.Count < period * 2 + 1) return null;

        var midIndex = candles.Count - period - 1;
        var midHigh = candles[midIndex].High;

        // Check if mid is higher than all candles in period before and after
        for (int i = midIndex - period; i < midIndex + period + 1; i++)
        {
            if (i != midIndex && candles[i].High >= midHigh)
                return null;
        }

        return midHigh;
    }

    public void UpdateParameters(Dictionary<string, object> newParameters)
    {
        Parameters = newParameters;
        
        if (Parameters.ContainsKey("swing_period"))
            _swingPeriod = Convert.ToInt32(Parameters["swing_period"]);
        if (Parameters.ContainsKey("min_swing_percent"))
            _minSwingPercent = Convert.ToDecimal(Parameters["min_swing_percent"]);
    }
}
