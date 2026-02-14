using BitgetApi.TradingEngine.Models;

namespace BitgetApi.TradingEngine.Strategies;

/// <summary>
/// RSI + Volume + EMA Strategy
/// Identifies oversold/overbought conditions with volume confirmation and EMA trend filter
/// </summary>
public class RsiVolumeEmaStrategy : IStrategy
{
    public string Name => "RSI_Volume_EMA";
    public string Market => "Futures";
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object> Parameters { get; set; }

    // Default parameters
    private int _rsiPeriod = 14;
    private int _rsiOversold = 30;
    private int _rsiOverbought = 70;
    private int _emaPeriod = 50;
    private decimal _volumeThreshold = 1.5m; // 1.5x average volume

    public RsiVolumeEmaStrategy(Dictionary<string, object>? parameters = null)
    {
        Parameters = parameters ?? new Dictionary<string, object>();
        IsEnabled = true;
        
        // Load parameters if provided
        if (Parameters.ContainsKey("rsi_period"))
            _rsiPeriod = Convert.ToInt32(Parameters["rsi_period"]);
        if (Parameters.ContainsKey("rsi_oversold"))
            _rsiOversold = Convert.ToInt32(Parameters["rsi_oversold"]);
        if (Parameters.ContainsKey("rsi_overbought"))
            _rsiOverbought = Convert.ToInt32(Parameters["rsi_overbought"]);
        if (Parameters.ContainsKey("ema_period"))
            _emaPeriod = Convert.ToInt32(Parameters["ema_period"]);
        if (Parameters.ContainsKey("volume_threshold"))
            _volumeThreshold = Convert.ToDecimal(Parameters["volume_threshold"]);
    }

    public async Task<Signal?> GenerateSignalAsync(string symbol, List<Candle> candles)
    {
        Console.WriteLine($"🔍 [RSI_Volume_EMA] Analyzing {symbol} with {candles.Count} candles");
        
        try
        {
            if (candles.Count < Math.Max(_rsiPeriod, _emaPeriod) + 20)
            {
                Console.WriteLine($"⚠️ [RSI_Volume_EMA] Not enough candles (need {Math.Max(_rsiPeriod, _emaPeriod) + 20})");
                return await Task.FromResult<Signal?>(null);
            }

            // Calculate indicators
            var rsi = CalculateRSI(candles, _rsiPeriod);
            var ema = CalculateEMA(candles, _emaPeriod);
            var avgVolume = candles.TakeLast(20).Average(c => c.Volume);
            var currentVolume = candles.Last().Volume;
            var currentPrice = candles.Last().Close;

            Console.WriteLine($"🔍 [RSI_Volume_EMA] RSI: {rsi:F2}, EMA: {ema:F2}, Price: {currentPrice:F2}, Vol: {currentVolume / avgVolume:F2}x avg");

            // LONG Signal: RSI oversold + price above EMA + high volume
            if (rsi < _rsiOversold && currentPrice > ema && currentVolume > avgVolume * _volumeThreshold)
            {
                var stopLoss = currentPrice * 0.97m; // 3% stop loss
                var takeProfit = currentPrice * 1.06m; // 6% take profit
                var confidence = Math.Min(95m, 100m - rsi); // Lower RSI = higher confidence

                Console.WriteLine($"✅ [RSI_Volume_EMA] LONG signal at RSI {rsi:F2}");

                return await Task.FromResult(new Signal
                {
                    Symbol = symbol,
                    Strategy = Name,
                    Type = SignalType.LONG,
                    EntryPrice = currentPrice,
                    StopLoss = stopLoss,
                    TakeProfit = takeProfit,
                    Confidence = (double)confidence,
                    Reason = $"RSI oversold ({rsi:F2}), price above EMA ({ema:F2}), high volume ({currentVolume / avgVolume:F2}x)",
                    Timestamp = DateTime.UtcNow
                });
            }

            // SHORT Signal: RSI overbought + price below EMA + high volume
            if (rsi > _rsiOverbought && currentPrice < ema && currentVolume > avgVolume * _volumeThreshold)
            {
                var stopLoss = currentPrice * 1.03m; // 3% stop loss
                var takeProfit = currentPrice * 0.94m; // 6% take profit
                var confidence = Math.Min(95m, rsi - 30m); // Higher RSI = higher confidence for short

                Console.WriteLine($"✅ [RSI_Volume_EMA] SHORT signal at RSI {rsi:F2}");

                return await Task.FromResult(new Signal
                {
                    Symbol = symbol,
                    Strategy = Name,
                    Type = SignalType.SHORT,
                    EntryPrice = currentPrice,
                    StopLoss = stopLoss,
                    TakeProfit = takeProfit,
                    Confidence = (double)confidence,
                    Reason = $"RSI overbought ({rsi:F2}), price below EMA ({ema:F2}), high volume ({currentVolume / avgVolume:F2}x)",
                    Timestamp = DateTime.UtcNow
                });
            }

            Console.WriteLine($"ℹ️ [RSI_Volume_EMA] No signal (RSI: {rsi:F2}, conditions not met)");
            return await Task.FromResult<Signal?>(null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [RSI_Volume_EMA] ERROR: {ex.Message}");
            throw;
        }
    }

    private decimal CalculateRSI(List<Candle> candles, int period)
    {
        var prices = candles.TakeLast(period + 1).Select(c => c.Close).ToList();
        
        var gains = new List<decimal>();
        var losses = new List<decimal>();

        for (int i = 1; i < prices.Count; i++)
        {
            var change = prices[i] - prices[i - 1];
            gains.Add(change > 0 ? change : 0);
            losses.Add(change < 0 ? Math.Abs(change) : 0);
        }

        var avgGain = gains.Average();
        var avgLoss = losses.Average();

        if (avgLoss == 0) return 100;

        var rs = avgGain / avgLoss;
        var rsi = 100 - (100 / (1 + rs));

        return rsi;
    }

    private decimal CalculateEMA(List<Candle> candles, int period)
    {
        var prices = candles.TakeLast(period).Select(c => c.Close).ToList();
        var multiplier = 2m / (period + 1);
        var ema = prices.First();

        foreach (var price in prices.Skip(1))
        {
            ema = (price - ema) * multiplier + ema;
        }

        return ema;
    }

    public void UpdateParameters(Dictionary<string, object> newParameters)
    {
        Parameters = newParameters;
        
        if (Parameters.ContainsKey("rsi_period"))
            _rsiPeriod = Convert.ToInt32(Parameters["rsi_period"]);
        if (Parameters.ContainsKey("rsi_oversold"))
            _rsiOversold = Convert.ToInt32(Parameters["rsi_oversold"]);
        if (Parameters.ContainsKey("rsi_overbought"))
            _rsiOverbought = Convert.ToInt32(Parameters["rsi_overbought"]);
        if (Parameters.ContainsKey("ema_period"))
            _emaPeriod = Convert.ToInt32(Parameters["ema_period"]);
        if (Parameters.ContainsKey("volume_threshold"))
            _volumeThreshold = Convert.ToDecimal(Parameters["volume_threshold"]);
    }
}
