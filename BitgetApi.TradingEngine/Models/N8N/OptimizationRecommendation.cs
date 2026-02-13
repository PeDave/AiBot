using System.Text.Json.Serialization;

namespace BitgetApi.TradingEngine.Models.N8N;

public class OptimizationRecommendation
{
    [JsonPropertyName("optimizations")]
    public List<ParameterOptimization> Optimizations { get; set; } = new();
}

public class ParameterOptimization
{
    [JsonPropertyName("strategy")]
    public string Strategy { get; set; } = string.Empty;
    
    [JsonPropertyName("parameter")]
    public string Parameter { get; set; } = string.Empty;
    
    [JsonPropertyName("currentValue")]
    public object? CurrentValue { get; set; }
    
    [JsonPropertyName("suggestedValue")]
    public object? SuggestedValue { get; set; }
    
    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
    
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}
