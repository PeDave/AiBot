namespace BitgetApi.TradingEngine.Indicators;

public class VolumeIndicator
{
    private readonly int _period;

    public VolumeIndicator(int period = 20)
    {
        _period = period;
    }

    public decimal CalculateAverage(List<Models.Candle> candles)
    {
        if (candles.Count < _period)
            return candles.Average(c => c.Volume);

        return candles.TakeLast(_period).Average(c => c.Volume);
    }

    public bool IsVolumeSpike(List<Models.Candle> candles, double multiplier = 1.5)
    {
        if (candles.Count == 0)
            return false;

        var avgVolume = CalculateAverage(candles);
        var currentVolume = candles.Last().Volume;

        return currentVolume > avgVolume * (decimal)multiplier;
    }

    public decimal GetCurrentVolume(List<Models.Candle> candles)
    {
        return candles.Count > 0 ? candles.Last().Volume : 0;
    }
}
