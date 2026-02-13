namespace BitgetApi.TradingEngine.Indicators;

public class RsiIndicator
{
    private readonly int _period;

    public RsiIndicator(int period = 14)
    {
        _period = period;
    }

    public double Calculate(List<Models.Candle> candles)
    {
        if (candles.Count < _period + 1)
            return 50; // Return neutral RSI if insufficient data

        var gains = new List<decimal>();
        var losses = new List<decimal>();

        for (int i = candles.Count - _period; i < candles.Count; i++)
        {
            var change = candles[i].Close - candles[i - 1].Close;
            if (change >= 0)
            {
                gains.Add(change);
                losses.Add(0);
            }
            else
            {
                gains.Add(0);
                losses.Add(Math.Abs(change));
            }
        }

        var avgGain = gains.Average();
        var avgLoss = losses.Average();

        if (avgLoss == 0)
            return 100;

        var rs = avgGain / avgLoss;
        var rsi = 100 - (100 / (1 + rs));

        return (double)rsi;
    }

    public List<double> CalculateSeries(List<Models.Candle> candles)
    {
        var rsiValues = new List<double>();
        
        for (int i = _period; i < candles.Count; i++)
        {
            var subset = candles.Take(i + 1).ToList();
            rsiValues.Add(Calculate(subset));
        }

        return rsiValues;
    }
}
