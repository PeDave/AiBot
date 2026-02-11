namespace BitgetApi.Dashboard.Models;

public class PriceUpdate
{
    public string Symbol { get; set; } = string.Empty;
    public decimal LastPrice { get; set; }
    public decimal High24h { get; set; }
    public decimal Low24h { get; set; }
    public decimal Volume24h { get; set; }
    public decimal Change24h { get; set; }
    public DateTime Timestamp { get; set; }
    
    // Backward compatibility alias
    public decimal Price => LastPrice;
    public string Volume => Volume24h.ToString("F8");
}
