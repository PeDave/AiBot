using BitgetApi.TradingEngine.Models;

namespace BitgetApi.TradingEngine.Strategies;

/// <summary>
/// Fair Value Gap (FVG) + Liquidity Strategy
/// Identifies price gaps (imbalances) and liquidity zones
/// </summary>
public class FvgLiquidityStrategy : IStrategy
{
    public string Name => "FVG_Liquidity";
    public string Market => "Futures";
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object> Parameters { get; set; }

    private decimal _minGapPercent = 0.5m; // Minimum 0.5% gap
    private int _lookbackPeriod = 50;

    public FvgLiquidityStrategy(Dictionary<string, object>? parameters = null)
    {
        Parameters = parameters ?? new Dictionary<string, object>();
        IsEnabled = true;
        
        if (Parameters.ContainsKey("min_gap_percent"))
            _minGapPercent = Convert.ToDecimal(Parameters["min_gap_percent"]);
        if (Parameters.ContainsKey("lookback_period"))
            _lookbackPeriod = Convert.ToInt32(Parameters["lookback_period"]);
    }

    public async Task<Signal?> GenerateSignalAsync(string symbol, List<Candle> candles)
    {
        Console.WriteLine($"🔍 [FVG_Liquidity] Analyzing {symbol} with {candles.Count} candles");
        
        try
        {
            if (candles.Count < _lookbackPeriod)
            {
                Console.WriteLine($"⚠️ [FVG_Liquidity] Not enough candles (need {_lookbackPeriod})");
                return await Task.FromResult<Signal?>(null);
            }

            var recentCandles = candles.TakeLast(_lookbackPeriod).ToList();
            var currentPrice = candles.Last().Close;

            // Find Fair Value Gaps (3-candle pattern)
            for (int i = recentCandles.Count - 3; i >= 0; i--)
            {
                var candle1 = recentCandles[i];
                var candle2 = recentCandles[i + 1];
                var candle3 = recentCandles[i + 2];

                // Bullish FVG: candle3.Low > candle1.High (gap up)
                var bullishGap = candle3.Low - candle1.High;
                if (bullishGap > 0)
                {
                    var gapPercent = (bullishGap / candle1.High) * 100;
                    
                    if (gapPercent >= _minGapPercent && currentPrice >= candle1.High && currentPrice <= candle3.Low)
                    {
                        var stopLoss = candle1.Low;
                        var riskAmount = currentPrice - stopLoss;
                        var takeProfit = currentPrice + (riskAmount * 2m); // 2:1 R:R
                        var confidence = Math.Min(90m, 60m + gapPercent * 5m);

                        Console.WriteLine($"✅ [FVG_Liquidity] Bullish FVG at {gapPercent:F2}% gap");

                        return await Task.FromResult(new Signal
                        {
                            Symbol = symbol,
                            Strategy = Name,
                            Type = SignalType.LONG,
                            EntryPrice = currentPrice,
                            StopLoss = stopLoss,
                            TakeProfit = takeProfit,
                            Confidence = (double)confidence,
                            Reason = $"Bullish FVG detected ({gapPercent:F2}% gap), price in fill zone [{candle1.High:F2} - {candle3.Low:F2}]",
                            Timestamp = DateTime.UtcNow
                        });
                    }
                }

                // Bearish FVG: candle1.Low > candle3.High (gap down)
                var bearishGap = candle1.Low - candle3.High;
                if (bearishGap > 0)
                {
                    var gapPercent = (bearishGap / candle1.Low) * 100;
                    
                    if (gapPercent >= _minGapPercent && currentPrice <= candle1.Low && currentPrice >= candle3.High)
                    {
                        var stopLoss = candle1.High;
                        var riskAmount = stopLoss - currentPrice;
                        var takeProfit = currentPrice - (riskAmount * 2m); // 2:1 R:R
                        var confidence = Math.Min(90m, 60m + gapPercent * 5m);

                        Console.WriteLine($"✅ [FVG_Liquidity] Bearish FVG at {gapPercent:F2}% gap");

                        return await Task.FromResult(new Signal
                        {
                            Symbol = symbol,
                            Strategy = Name,
                            Type = SignalType.SHORT,
                            EntryPrice = currentPrice,
                            StopLoss = stopLoss,
                            TakeProfit = takeProfit,
                            Confidence = (double)confidence,
                            Reason = $"Bearish FVG detected ({gapPercent:F2}% gap), price in fill zone [{candle3.High:F2} - {candle1.Low:F2}]",
                            Timestamp = DateTime.UtcNow
                        });
                    }
                }
            }

            Console.WriteLine($"ℹ️ [FVG_Liquidity] No FVG detected");
            return await Task.FromResult<Signal?>(null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [FVG_Liquidity] ERROR: {ex.Message}");
            throw;
        }
    }

    public void UpdateParameters(Dictionary<string, object> newParameters)
    {
        Parameters = newParameters;
        
        if (Parameters.ContainsKey("min_gap_percent"))
            _minGapPercent = Convert.ToDecimal(Parameters["min_gap_percent"]);
        if (Parameters.ContainsKey("lookback_period"))
            _lookbackPeriod = Convert.ToInt32(Parameters["lookback_period"]);
    }
}
