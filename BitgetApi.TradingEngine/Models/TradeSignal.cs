namespace BitgetApi.TradingEngine.Models;

public class TradeSignal
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Strategy { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // LONG, SHORT, CLOSE
    public decimal EntryPrice { get; set; }
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
    public decimal Confidence { get; set; }
    public string Reason { get; set; } = string.Empty;
}
