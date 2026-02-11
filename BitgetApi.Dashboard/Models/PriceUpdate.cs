namespace BitgetApi.Dashboard.Models;

public class PriceUpdate
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Change24h { get; set; }
    public string Volume { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
