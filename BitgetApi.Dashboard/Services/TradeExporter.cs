using BitgetApi.Dashboard.Models;

namespace BitgetApi.Dashboard.Services;

public class TradeExporter
{
    private readonly string _basePath;
    
    public TradeExporter(string? basePath = null)
    {
        _basePath = basePath ?? Directory.GetCurrentDirectory();
    }
    
    public void ExportToCsv(List<TradeRecord> trades, string? filename = null)
    {
        if (trades == null || trades.Count == 0)
        {
            return;
        }
        
        filename ??= $"trades_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var fullPath = Path.Combine(_basePath, filename);
        
        using var writer = new StreamWriter(fullPath, append: false);
        
        // Write header
        writer.WriteLine("Timestamp,Symbol,Side,Price,Size,TradeId");
        
        // Write trade records
        foreach (var trade in trades)
        {
            writer.WriteLine($"{trade.Timestamp:yyyy-MM-dd HH:mm:ss},{trade.Symbol},{trade.Side},{trade.Price},{trade.Size},{trade.TradeId}");
        }
    }
    
    public void AppendToCsv(TradeRecord trade, string filename = "trades.csv")
    {
        var fullPath = Path.Combine(_basePath, filename);
        var fileExists = File.Exists(fullPath);
        
        using var writer = new StreamWriter(fullPath, append: true);
        
        // Write header if file doesn't exist
        if (!fileExists)
        {
            writer.WriteLine("Timestamp,Symbol,Side,Price,Size,TradeId");
        }
        
        writer.WriteLine($"{trade.Timestamp:yyyy-MM-dd HH:mm:ss},{trade.Symbol},{trade.Side},{trade.Price},{trade.Size},{trade.TradeId}");
    }
}
