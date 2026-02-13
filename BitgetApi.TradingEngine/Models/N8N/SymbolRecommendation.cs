using System.Text.Json.Serialization;

namespace BitgetApi.TradingEngine.Models.N8N;

public class SymbolRecommendation
{
    [JsonPropertyName("selectedSymbols")]
    public List<string> SelectedSymbols { get; set; } = new();
    
    [JsonPropertyName("reasoning")]
    public Dictionary<string, string> Reasoning { get; set; } = new();
}
