using BitgetApi.TradingEngine.Models;
using BitgetApi.TradingEngine.Indicators;

namespace BitgetApi.TradingEngine.Strategies;

public class SwingStrategy : IStrategy
{
    public string Name => "Swing";
    public string Market => "Futures";
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object> Parameters { get; set; }

    private readonly VolumeIndicator _volumeIndicator;

    public SwingStrategy(Dictionary<string, object>? parameters = null)
    {
        Parameters = parameters ?? new Dictionary<string, object>
        {
            { "swing_lookback", 20 },
            { "min_swing_distance_percent", 2.0 },
            { "volume_multiplier", 1.3 },
            { "tp_ratio", 1.0 },
            { "sl_ratio", 0.5 }
        };

        _volumeIndicator = new VolumeIndicator();
    }

    public Task<Signal?> GenerateSignalAsync(string symbol, List<Candle> candles)
    {
        if (candles.Count < 50)
            return Task.FromResult<Signal?>(null);

        var lookback = GetParameter<int>("swing_lookback");
        var minDistancePercent = GetParameter<double>("min_swing_distance_percent");
        var volumeMultiplier = GetParameter<double>("volume_multiplier");
        var tpRatio = GetParameter<double>("tp_ratio");
        var slRatio = GetParameter<double>("sl_ratio");

        var swingHighs = FindSwingHighs(candles, lookback);
        var swingLows = FindSwingLows(candles, lookback);
        var currentPrice = candles.Last().Close;
        var volumeSpike = _volumeIndicator.IsVolumeSpike(candles, volumeMultiplier);

        // Check for bounce from swing low (Long setup)
        var lastSwingLow = swingLows.LastOrDefault();
        if (lastSwingLow > 0)
        {
            var distancePercent = (double)((currentPrice - lastSwingLow) / lastSwingLow * 100);
            
            // Price bouncing from swing low with volume
            if (distancePercent >= 0 && distancePercent <= minDistancePercent && volumeSpike)
            {
                var prevSwingHigh = swingHighs.LastOrDefault();
                var targetPrice = prevSwingHigh > 0 ? prevSwingHigh : currentPrice * (1 + (decimal)minDistancePercent / 100);
                var distance = targetPrice - currentPrice;

                var signal = new Signal
                {
                    Symbol = symbol,
                    Strategy = Name,
                    Type = SignalType.LONG,
                    EntryPrice = currentPrice,
                    StopLoss = lastSwingLow * (1 - (decimal)(slRatio / 100)),
                    TakeProfit = currentPrice + (distance * (decimal)tpRatio),
                    Confidence = CalculateConfidence(distancePercent, volumeSpike, true),
                    Reason = $"Bounce from swing low at {lastSwingLow:F2}, target swing high {targetPrice:F2}"
                };

                signal.Metadata["swing_low"] = lastSwingLow;
                signal.Metadata["swing_high"] = prevSwingHigh;

                return Task.FromResult<Signal?>(signal);
            }
        }

        // Check for rejection from swing high (Short setup)
        var lastSwingHigh = swingHighs.LastOrDefault();
        if (lastSwingHigh > 0)
        {
            var distancePercent = (double)((lastSwingHigh - currentPrice) / lastSwingHigh * 100);
            
            // Price rejecting from swing high with volume
            if (distancePercent >= 0 && distancePercent <= minDistancePercent && volumeSpike)
            {
                var prevSwingLow = swingLows.LastOrDefault();
                var targetPrice = prevSwingLow > 0 ? prevSwingLow : currentPrice * (1 - (decimal)minDistancePercent / 100);
                var distance = currentPrice - targetPrice;

                var signal = new Signal
                {
                    Symbol = symbol,
                    Strategy = Name,
                    Type = SignalType.SHORT,
                    EntryPrice = currentPrice,
                    StopLoss = lastSwingHigh * (1 + (decimal)(slRatio / 100)),
                    TakeProfit = currentPrice - (distance * (decimal)tpRatio),
                    Confidence = CalculateConfidence(distancePercent, volumeSpike, false),
                    Reason = $"Rejection from swing high at {lastSwingHigh:F2}, target swing low {targetPrice:F2}"
                };

                signal.Metadata["swing_high"] = lastSwingHigh;
                signal.Metadata["swing_low"] = prevSwingLow;

                return Task.FromResult<Signal?>(signal);
            }
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

    private List<decimal> FindSwingHighs(List<Candle> candles, int lookback)
    {
        var swingHighs = new List<decimal>();

        for (int i = lookback; i < candles.Count - lookback; i++)
        {
            var isSwingHigh = true;
            
            for (int j = 1; j <= lookback; j++)
            {
                if (candles[i].High <= candles[i - j].High || candles[i].High <= candles[i + j].High)
                {
                    isSwingHigh = false;
                    break;
                }
            }

            if (isSwingHigh)
            {
                swingHighs.Add(candles[i].High);
            }
        }

        return swingHighs;
    }

    private List<decimal> FindSwingLows(List<Candle> candles, int lookback)
    {
        var swingLows = new List<decimal>();

        for (int i = lookback; i < candles.Count - lookback; i++)
        {
            var isSwingLow = true;
            
            for (int j = 1; j <= lookback; j++)
            {
                if (candles[i].Low >= candles[i - j].Low || candles[i].Low >= candles[i + j].Low)
                {
                    isSwingLow = false;
                    break;
                }
            }

            if (isSwingLow)
            {
                swingLows.Add(candles[i].Low);
            }
        }

        return swingLows;
    }

    private double CalculateConfidence(double distancePercent, bool volumeSpike, bool isLong)
    {
        var confidence = 65.0;

        // Closer to swing level = higher confidence
        confidence += (2.0 - distancePercent) * 5;

        // Volume confirmation
        if (volumeSpike)
            confidence += 15;

        return Math.Min(confidence, 90);
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
