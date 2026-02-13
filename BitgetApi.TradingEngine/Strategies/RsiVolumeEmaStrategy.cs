using BitgetApi.TradingEngine.Models;
using BitgetApi.TradingEngine.Indicators;

namespace BitgetApi.TradingEngine.Strategies;

public class RsiVolumeEmaStrategy : IStrategy
{
    public string Name => "RSI_Volume_EMA";
    public string Market => "Futures";
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object> Parameters { get; set; }

    private readonly RsiIndicator _rsiIndicator;
    private readonly EmaIndicator _emaIndicator;
    private readonly VolumeIndicator _volumeIndicator;

    public RsiVolumeEmaStrategy(Dictionary<string, object>? parameters = null)
    {
        Parameters = parameters ?? new Dictionary<string, object>
        {
            { "rsi_period", 14 },
            { "rsi_oversold", 30.0 },
            { "rsi_overbought", 70.0 },
            { "ema_period", 50 },
            { "volume_multiplier", 1.5 },
            { "tp_percent", 2.0 },
            { "sl_percent", 1.0 }
        };

        _rsiIndicator = new RsiIndicator(GetParameter<int>("rsi_period"));
        _emaIndicator = new EmaIndicator(GetParameter<int>("ema_period"));
        _volumeIndicator = new VolumeIndicator();
    }

    public Task<Signal?> GenerateSignalAsync(string symbol, List<Candle> candles)
    {
        if (candles.Count < 100)
            return Task.FromResult<Signal?>(null);

        var rsi = _rsiIndicator.Calculate(candles);
        var ema = _emaIndicator.Calculate(candles);
        var currentPrice = candles.Last().Close;
        var volumeSpike = _volumeIndicator.IsVolumeSpike(candles, GetParameter<double>("volume_multiplier"));

        var rsiOversold = GetParameter<double>("rsi_oversold");
        var rsiOverbought = GetParameter<double>("rsi_overbought");
        var tpPercent = GetParameter<double>("tp_percent");
        var slPercent = GetParameter<double>("sl_percent");

        // Long Entry: RSI < oversold, Volume spike, Price > EMA
        if (rsi < rsiOversold && volumeSpike && currentPrice > ema)
        {
            var signal = new Signal
            {
                Symbol = symbol,
                Strategy = Name,
                Type = SignalType.LONG,
                EntryPrice = currentPrice,
                StopLoss = currentPrice * (1 - (decimal)(slPercent / 100)),
                TakeProfit = currentPrice * (1 + (decimal)(tpPercent / 100)),
                Confidence = CalculateConfidence(rsi, volumeSpike, true),
                Reason = $"RSI oversold ({rsi:F1}), Volume spike ({_volumeIndicator.GetCurrentVolume(candles):F0}), Price above EMA({ema:F2})"
            };

            return Task.FromResult<Signal?>(signal);
        }

        // Short Entry: RSI > overbought, Volume spike, Price < EMA
        if (rsi > rsiOverbought && volumeSpike && currentPrice < ema)
        {
            var signal = new Signal
            {
                Symbol = symbol,
                Strategy = Name,
                Type = SignalType.SHORT,
                EntryPrice = currentPrice,
                StopLoss = currentPrice * (1 + (decimal)(slPercent / 100)),
                TakeProfit = currentPrice * (1 - (decimal)(tpPercent / 100)),
                Confidence = CalculateConfidence(rsi, volumeSpike, false),
                Reason = $"RSI overbought ({rsi:F1}), Volume spike ({_volumeIndicator.GetCurrentVolume(candles):F0}), Price below EMA({ema:F2})"
            };

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

    private double CalculateConfidence(double rsi, bool volumeSpike, bool isLong)
    {
        var confidence = 50.0;

        // RSI strength (max 30 points)
        if (isLong)
        {
            confidence += (30 - rsi) * 0.5; // More oversold = higher confidence
        }
        else
        {
            confidence += (rsi - 70) * 0.5; // More overbought = higher confidence
        }

        // Volume spike (20 points)
        if (volumeSpike)
            confidence += 20;

        return Math.Min(confidence, 95); // Cap at 95%
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
