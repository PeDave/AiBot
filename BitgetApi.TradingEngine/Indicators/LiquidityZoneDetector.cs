namespace BitgetApi.TradingEngine.Indicators;

public class LiquidityZone
{
    public decimal Price { get; set; }
    public int TouchCount { get; set; }
    public bool IsSupport { get; set; }
    public bool IsResistance { get; set; }
    public DateTime FirstTouch { get; set; }
    public DateTime LastTouch { get; set; }
    public decimal Strength { get; set; } // Based on touch count and volume
}

public class LiquidityZoneDetector
{
    private readonly int _lookbackPeriod;
    private readonly double _priceTolerancePercent;

    public LiquidityZoneDetector(int lookbackPeriod = 50, double priceTolerancePercent = 0.5)
    {
        _lookbackPeriod = lookbackPeriod;
        _priceTolerancePercent = priceTolerancePercent;
    }

    public List<LiquidityZone> DetectZones(List<Models.Candle> candles)
    {
        var zones = new List<LiquidityZone>();

        if (candles.Count < _lookbackPeriod)
            return zones;

        var recentCandles = candles.TakeLast(_lookbackPeriod).ToList();
        
        // Find swing highs and lows
        var swingHighs = FindSwingHighs(recentCandles);
        var swingLows = FindSwingLows(recentCandles);

        // Group nearby swing points into zones
        zones.AddRange(GroupIntoZones(swingHighs, candles, isResistance: true));
        zones.AddRange(GroupIntoZones(swingLows, candles, isResistance: false));

        return zones.OrderByDescending(z => z.Strength).ToList();
    }

    private List<decimal> FindSwingHighs(List<Models.Candle> candles)
    {
        var swingHighs = new List<decimal>();

        for (int i = 2; i < candles.Count - 2; i++)
        {
            if (candles[i].High > candles[i - 1].High &&
                candles[i].High > candles[i - 2].High &&
                candles[i].High > candles[i + 1].High &&
                candles[i].High > candles[i + 2].High)
            {
                swingHighs.Add(candles[i].High);
            }
        }

        return swingHighs;
    }

    private List<decimal> FindSwingLows(List<Models.Candle> candles)
    {
        var swingLows = new List<decimal>();

        for (int i = 2; i < candles.Count - 2; i++)
        {
            if (candles[i].Low < candles[i - 1].Low &&
                candles[i].Low < candles[i - 2].Low &&
                candles[i].Low < candles[i + 1].Low &&
                candles[i].Low < candles[i + 2].Low)
            {
                swingLows.Add(candles[i].Low);
            }
        }

        return swingLows;
    }

    private List<LiquidityZone> GroupIntoZones(List<decimal> prices, List<Models.Candle> candles, bool isResistance)
    {
        var zones = new List<LiquidityZone>();
        var grouped = new Dictionary<decimal, List<decimal>>();

        // Group prices within tolerance
        foreach (var price in prices)
        {
            var foundGroup = false;
            foreach (var key in grouped.Keys.ToList())
            {
                var tolerance = key * (decimal)_priceTolerancePercent / 100;
                if (Math.Abs(price - key) <= tolerance)
                {
                    grouped[key].Add(price);
                    foundGroup = true;
                    break;
                }
            }

            if (!foundGroup)
            {
                grouped[price] = new List<decimal> { price };
            }
        }

        // Create zones from groups
        foreach (var group in grouped.Where(g => g.Value.Count >= 2))
        {
            var avgPrice = group.Value.Average();
            var touchCount = group.Value.Count;

            zones.Add(new LiquidityZone
            {
                Price = avgPrice,
                TouchCount = touchCount,
                IsSupport = !isResistance,
                IsResistance = isResistance,
                FirstTouch = candles.First().Timestamp,
                LastTouch = candles.Last().Timestamp,
                Strength = touchCount * 10 // Simple strength calculation
            });
        }

        return zones;
    }

    public bool IsNearLiquidityZone(decimal currentPrice, List<LiquidityZone> zones, double tolerancePercent = 1.0)
    {
        foreach (var zone in zones)
        {
            var tolerance = zone.Price * (decimal)tolerancePercent / 100;
            if (Math.Abs(currentPrice - zone.Price) <= tolerance)
            {
                return true;
            }
        }

        return false;
    }
}
