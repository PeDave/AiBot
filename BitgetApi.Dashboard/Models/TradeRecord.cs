namespace BitgetApi.Dashboard.Models;

public class TradeRecord
{
    public string Symbol { get; set; } = string.Empty;
    public string TradeId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Size { get; set; }
    public string Side { get; set; } = string.Empty; // "buy" or "sell"
    public DateTime Timestamp { get; set; }
}
