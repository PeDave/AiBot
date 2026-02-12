using System.ComponentModel.DataAnnotations;

namespace BitgetApi.Dashboard.Web.Data.Models;

public class CandleData
{
    [Required]
    public string Symbol { get; set; } = "";
    
    [Required]
    public string Interval { get; set; } = ""; // 15m, 1h, 4h, 1d
    
    [Required]
    public long Timestamp { get; set; }
    
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public decimal QuoteVolume { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
