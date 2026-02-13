namespace BitgetApi.TradingEngine.Indicators;

public class SmaIndicator
{
    private readonly int _period;

    public SmaIndicator(int period = 200)
    {
        _period = period;
    }

    public decimal Calculate(List<Models.Candle> candles)
    {
        if (candles.Count < _period)
            return candles.Average(c => c.Close); // Use available data if insufficient

        return candles.TakeLast(_period).Average(c => c.Close);
    }

    public List<decimal> CalculateSeries(List<Models.Candle> candles)
    {
        var smaValues = new List<decimal>();
        
        for (int i = _period - 1; i < candles.Count; i++)
        {
            var subset = candles.Skip(i - _period + 1).Take(_period).ToList();
            smaValues.Add(subset.Average(c => c.Close));
        }

        return smaValues;
    }
}
