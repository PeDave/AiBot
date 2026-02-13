using System.Text.Json.Serialization;

namespace BitgetApi.TradingEngine.Models.N8N;

public class AgentDecision
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;
    
    [JsonPropertyName("decision")]
    public string Decision { get; set; } = string.Empty; // "EXECUTE" or "NO_ACTION"
    
    [JsonPropertyName("trade")]
    public TradeDecision? Trade { get; set; }
    
    [JsonPropertyName("strategyScores")]
    public Dictionary<string, double> StrategyScores { get; set; } = new();
    
    [JsonPropertyName("reasoning")]
    public string Reasoning { get; set; } = string.Empty;
}

public class TradeDecision
{
    [JsonPropertyName("direction")]
    public string Direction { get; set; } = string.Empty; // "LONG" or "SHORT"
    
    [JsonPropertyName("entryPrice")]
    public decimal EntryPrice { get; set; }
    
    [JsonPropertyName("stopLoss")]
    public decimal StopLoss { get; set; }
    
    [JsonPropertyName("takeProfit")]
    public decimal TakeProfit { get; set; }
    
    [JsonPropertyName("positionSizeUsd")]
    public decimal PositionSizeUsd { get; set; }
    
    [JsonPropertyName("leverage")]
    public int Leverage { get; set; }
    
    [JsonPropertyName("confidence")]
    public double Confidence { get; set; }
}
