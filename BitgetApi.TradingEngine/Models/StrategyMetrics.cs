namespace BitgetApi.TradingEngine.Models;

public class StrategyMetrics
{
    public int Id { get; set; }
    public string StrategyName { get; set; } = string.Empty;
    public double WinRate { get; set; }
    public double Roi { get; set; }
    public double MaxDrawdown { get; set; }
    public int TotalTrades { get; set; }
    public int WinningTrades { get; set; }
    public int LosingTrades { get; set; }
    public double AverageWinPercent { get; set; }
    public double AverageLossPercent { get; set; }
    public Dictionary<string, object> CurrentParameters { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
