using BitgetApi.TradingEngine.Models;

namespace BitgetApi.TradingEngine.Strategies;

public interface IStrategy
{
    string Name { get; }
    string Market { get; } // "Futures" or "Spot"
    bool IsEnabled { get; set; }
    Dictionary<string, object> Parameters { get; set; }
    
    Task<Signal?> GenerateSignalAsync(string symbol, List<Candle> candles);
    void UpdateParameters(Dictionary<string, object> newParameters);
}
