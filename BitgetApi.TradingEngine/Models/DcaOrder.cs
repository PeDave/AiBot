namespace BitgetApi.TradingEngine.Models;

public enum DcaOrderType
{
    Weekly,
    Daily,
    LiquidityZone
}

public class DcaOrder
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal AmountUsd { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public DcaOrderType Type { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public string OrderId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
