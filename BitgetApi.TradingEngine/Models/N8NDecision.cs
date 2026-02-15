namespace BitgetApi.TradingEngine.Models;

public class N8NDecision
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty; // EXECUTE, NO_ACTION
    public string Reasoning { get; set; } = string.Empty;
    public string? Trade { get; set; } // JSON of trade details
}
