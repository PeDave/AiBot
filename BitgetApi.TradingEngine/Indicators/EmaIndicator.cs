namespace BitgetApi.TradingEngine.Indicators;

public class EmaIndicator
{
    private readonly int _period;

    public EmaIndicator(int period = 50)
    {
        _period = period;
    }

    public decimal Calculate(List<Models.Candle> candles)
    {
        if (candles.Count < _period)
            return candles.Last().Close; // Return last close if insufficient data

        var multiplier = 2.0m / (_period + 1);
        
        // Calculate SMA for first EMA value
        var sma = candles.Take(_period).Average(c => c.Close);
        var ema = sma;

        // Calculate EMA for remaining values
        for (int i = _period; i < candles.Count; i++)
        {
            ema = (candles[i].Close - ema) * multiplier + ema;
        }

        return ema;
    }

    public List<decimal> CalculateSeries(List<Models.Candle> candles)
    {
        var emaValues = new List<decimal>();
        
        if (candles.Count < _period)
            return emaValues;

        var multiplier = 2.0m / (_period + 1);
        var sma = candles.Take(_period).Average(c => c.Close);
        var ema = sma;
        emaValues.Add(ema);

        for (int i = _period; i < candles.Count; i++)
        {
            ema = (candles[i].Close - ema) * multiplier + ema;
            emaValues.Add(ema);
        }

        return emaValues;
    }
}
