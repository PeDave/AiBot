using System.ComponentModel.DataAnnotations;

namespace BitgetApi.Dashboard.Web.Data.Models;

public class SymbolInfo
{
    [Key]
    public string Symbol { get; set; } = "";
    
    public string BaseCoin { get; set; } = "";
    public string QuoteCoin { get; set; } = "";
    public string ProductType { get; set; } = ""; // SPOT or FUTURES
    public string Status { get; set; } = "";
    public decimal MinTradeAmount { get; set; }
    public int PricePrecision { get; set; }
    public int QuantityPrecision { get; set; }
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
