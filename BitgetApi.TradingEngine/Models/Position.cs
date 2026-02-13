namespace BitgetApi.TradingEngine.Models;

public enum PositionStatus
{
    Open,
    Closed,
    PartiallyFilled
}

public class Position
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Strategy { get; set; } = string.Empty;
    public SignalType Side { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal Size { get; set; }
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
    public decimal? ExitPrice { get; set; }
    public PositionStatus Status { get; set; } = PositionStatus.Open;
    public DateTime OpenTime { get; set; } = DateTime.UtcNow;
    public DateTime? CloseTime { get; set; }
    public decimal PnL { get; set; }
    public decimal PnLPercent { get; set; }
    public int? Leverage { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public string? CloseOrderId { get; set; }
    public string Market { get; set; } = string.Empty; // "Futures" or "Spot"
}
