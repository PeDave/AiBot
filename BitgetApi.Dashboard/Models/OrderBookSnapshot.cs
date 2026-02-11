namespace BitgetApi.Dashboard.Models;

public class OrderBookSnapshot
{
    public string Symbol { get; set; } = string.Empty;
    public List<(decimal Price, decimal Size)> Bids { get; set; } = new();
    public List<(decimal Price, decimal Size)> Asks { get; set; } = new();
    public DateTime Timestamp { get; set; }
}
