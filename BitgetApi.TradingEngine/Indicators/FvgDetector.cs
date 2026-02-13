namespace BitgetApi.TradingEngine.Indicators;

public class FairValueGap
{
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public decimal GapLow { get; set; }
    public decimal GapHigh { get; set; }
    public decimal GapSize { get; set; }
    public bool IsBullish { get; set; }
    public bool IsFilled { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class FvgDetector
{
    private readonly double _minGapPercent;

    public FvgDetector(double minGapPercent = 0.5)
    {
        _minGapPercent = minGapPercent;
    }

    public List<FairValueGap> DetectFVGs(List<Models.Candle> candles)
    {
        var fvgs = new List<FairValueGap>();

        if (candles.Count < 3)
            return fvgs;

        for (int i = 2; i < candles.Count; i++)
        {
            var candle1 = candles[i - 2];
            var candle2 = candles[i - 1];
            var candle3 = candles[i];

            // Bullish FVG: Gap between candle1.High and candle3.Low
            if (candle3.Low > candle1.High)
            {
                var gapSize = candle3.Low - candle1.High;
                var gapPercent = (double)(gapSize / candle1.High * 100);

                if (gapPercent >= _minGapPercent)
                {
                    fvgs.Add(new FairValueGap
                    {
                        StartIndex = i - 2,
                        EndIndex = i,
                        GapLow = candle1.High,
                        GapHigh = candle3.Low,
                        GapSize = gapSize,
                        IsBullish = true,
                        IsFilled = false,
                        CreatedAt = candle3.Timestamp
                    });
                }
            }

            // Bearish FVG: Gap between candle1.Low and candle3.High
            if (candle3.High < candle1.Low)
            {
                var gapSize = candle1.Low - candle3.High;
                var gapPercent = (double)(gapSize / candle1.Low * 100);

                if (gapPercent >= _minGapPercent)
                {
                    fvgs.Add(new FairValueGap
                    {
                        StartIndex = i - 2,
                        EndIndex = i,
                        GapLow = candle3.High,
                        GapHigh = candle1.Low,
                        GapSize = gapSize,
                        IsBullish = false,
                        IsFilled = false,
                        CreatedAt = candle3.Timestamp
                    });
                }
            }
        }

        return fvgs;
    }

    public bool IsRetestingFVG(List<Models.Candle> candles, FairValueGap fvg, double tolerancePercent = 0.2)
    {
        if (candles.Count == 0)
            return false;

        var currentPrice = candles.Last().Close;
        var tolerance = fvg.GapSize * (decimal)tolerancePercent;

        // Check if price is within FVG range with tolerance
        return currentPrice >= (fvg.GapLow - tolerance) && 
               currentPrice <= (fvg.GapHigh + tolerance);
    }
}
