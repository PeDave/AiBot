using System.ComponentModel.DataAnnotations;

namespace BitgetApi.Dashboard.Web.Data.Models;

public class TradeHistory
{
    [Required]
    public string Symbol { get; set; } = "";
    
    [Required]
    public string TradeId { get; set; } = "";
    
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public long Timestamp { get; set; }
    public string Side { get; set; } = ""; // buy or sell
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
