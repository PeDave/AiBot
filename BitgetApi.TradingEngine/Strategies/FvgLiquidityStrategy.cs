using BitgetApi.TradingEngine.Models;
using BitgetApi.TradingEngine.Indicators;

namespace BitgetApi.TradingEngine.Strategies;

public class FvgLiquidityStrategy : IStrategy
{
    public string Name => "FVG_Liquidity";
    public string Market => "Futures";
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object> Parameters { get; set; }

    private readonly FvgDetector _fvgDetector;
    private readonly LiquidityZoneDetector _liquidityDetector;

    public FvgLiquidityStrategy(Dictionary<string, object>? parameters = null)
    {
        Parameters = parameters ?? new Dictionary<string, object>
        {
            { "fvg_min_gap_percent", 0.5 },
            { "liquidity_lookback", 50 },
            { "retest_tolerance_percent", 0.2 },
            { "tp_multiplier", 1.5 },
            { "sl_percent", 1.5 }
        };

        _fvgDetector = new FvgDetector(GetParameter<double>("fvg_min_gap_percent"));
        _liquidityDetector = new LiquidityZoneDetector(GetParameter<int>("liquidity_lookback"));
    }

    public Task<Signal?> GenerateSignalAsync(string symbol, List<Candle> candles)
    {
        if (candles.Count < 100)
            return Task.FromResult<Signal?>(null);

        var fvgs = _fvgDetector.DetectFVGs(candles);
        var liquidityZones = _liquidityDetector.DetectZones(candles);
        var currentPrice = candles.Last().Close;

        // Look for unfilled FVGs
        var recentFvgs = fvgs.Where(f => !f.IsFilled)
                              .OrderByDescending(f => f.CreatedAt)
                              .Take(5)
                              .ToList();

        foreach (var fvg in recentFvgs)
        {
            var isRetesting = _fvgDetector.IsRetestingFVG(candles, fvg, GetParameter<double>("retest_tolerance_percent"));
            var nearLiquidity = _liquidityDetector.IsNearLiquidityZone(currentPrice, liquidityZones);

            if (isRetesting && nearLiquidity)
            {
                var slPercent = GetParameter<double>("sl_percent");
                var tpMultiplier = GetParameter<double>("tp_multiplier");

                if (fvg.IsBullish)
                {
                    // Long entry on bullish FVG retest near support
                    var signal = new Signal
                    {
                        Symbol = symbol,
                        Strategy = Name,
                        Type = SignalType.LONG,
                        EntryPrice = currentPrice,
                        StopLoss = fvg.GapLow * (1 - (decimal)(slPercent / 100)),
                        TakeProfit = currentPrice + (fvg.GapSize * (decimal)tpMultiplier),
                        Confidence = CalculateConfidence(fvg, liquidityZones, currentPrice),
                        Reason = $"Bullish FVG retest at {fvg.GapLow:F2}-{fvg.GapHigh:F2}, near liquidity zone"
                    };

                    signal.Metadata["fvg_gap_size"] = fvg.GapSize;
                    signal.Metadata["fvg_created"] = fvg.CreatedAt;

                    return Task.FromResult<Signal?>(signal);
                }
                else
                {
                    // Short entry on bearish FVG retest near resistance
                    var signal = new Signal
                    {
                        Symbol = symbol,
                        Strategy = Name,
                        Type = SignalType.SHORT,
                        EntryPrice = currentPrice,
                        StopLoss = fvg.GapHigh * (1 + (decimal)(slPercent / 100)),
                        TakeProfit = currentPrice - (fvg.GapSize * (decimal)tpMultiplier),
                        Confidence = CalculateConfidence(fvg, liquidityZones, currentPrice),
                        Reason = $"Bearish FVG retest at {fvg.GapLow:F2}-{fvg.GapHigh:F2}, near liquidity zone"
                    };

                    signal.Metadata["fvg_gap_size"] = fvg.GapSize;
                    signal.Metadata["fvg_created"] = fvg.CreatedAt;

                    return Task.FromResult<Signal?>(signal);
                }
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

    private double CalculateConfidence(FairValueGap fvg, List<LiquidityZone> zones, decimal currentPrice)
    {
        var confidence = 70.0;

        // FVG size matters (larger gaps = higher confidence)
        var gapPercent = (double)(fvg.GapSize / fvg.GapLow * 100);
        confidence += Math.Min(gapPercent * 2, 15);

        // Proximity to liquidity zone
        var nearestZone = zones.OrderBy(z => Math.Abs(z.Price - currentPrice)).FirstOrDefault();
        if (nearestZone != null)
        {
            confidence += Math.Min(nearestZone.Strength / 2, 10);
        }

        return Math.Min(confidence, 95);
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
