using System.Globalization;

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
    
    // Backward compatibility properties
    // Note: Price is an alias for LastPrice to maintain compatibility with existing UI code
    public decimal Price => LastPrice;
    
    // Note: Volume returns the string representation of Volume24h. Original format was a string
    // from the ticker API. This maintains API compatibility but the string format may differ slightly.
    public string Volume => Volume24h.ToString(CultureInfo.InvariantCulture);
}
