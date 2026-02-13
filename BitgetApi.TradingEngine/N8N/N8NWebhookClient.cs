using System.Text;
using System.Text.Json;
using BitgetApi.TradingEngine.Models;
using BitgetApi.TradingEngine.Models.N8N;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BitgetApi.TradingEngine.N8N;

public class N8NWebhookClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<N8NWebhookClient> _logger;
    private readonly string _baseUrl;

    public N8NWebhookClient(
        HttpClient httpClient, 
        IConfiguration configuration,
        ILogger<N8NWebhookClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _baseUrl = configuration["N8N:WebhookBaseUrl"] ?? "";
    }

    public async Task<AgentDecision?> SendStrategyAnalysisAsync(string symbol, List<Signal> signals, Dictionary<string, object> marketData)
    {
        try
        {
            var webhook = _configuration["N8N:StrategyAnalysisWebhook"];
            var url = $"{_baseUrl}{webhook}";

            var payload = new
            {
                symbol,
                signals = signals.Select(s => new
                {
                    strategy = s.Strategy,
                    type = s.Type.ToString(),
                    entryPrice = s.EntryPrice,
                    stopLoss = s.StopLoss,
                    takeProfit = s.TakeProfit,
                    confidence = s.Confidence,
                    reason = s.Reason
                }),
                marketData
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending strategy analysis to N8N for {Symbol}", symbol);

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var decision = JsonSerializer.Deserialize<AgentDecision>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Received decision from N8N: {Decision} for {Symbol}", 
                    decision?.Decision ?? "UNKNOWN", symbol);

                return decision;
            }

            _logger.LogWarning("Failed to get decision from N8N: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending strategy analysis to N8N for {Symbol}", symbol);
            return null;
        }
    }

    public async Task<bool> SendPerformanceMetricsAsync(List<StrategyMetrics> metrics, Dictionary<string, object> overallPerformance)
    {
        try
        {
            var webhook = _configuration["N8N:PerformanceWebhook"];
            var url = $"{_baseUrl}{webhook}";

            var payload = new
            {
                strategies = metrics.Select(m => new
                {
                    name = m.StrategyName,
                    winRate = m.WinRate,
                    roi = m.Roi,
                    drawdown = m.MaxDrawdown,
                    totalTrades = m.TotalTrades,
                    parameters = m.CurrentParameters
                }),
                overallPerformance
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending performance metrics to N8N");

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Performance metrics sent successfully");
                return true;
            }

            _logger.LogWarning("Failed to send performance metrics: {StatusCode}", response.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending performance metrics to N8N");
            return false;
        }
    }

    public async Task<bool> SendSymbolUpdateAsync(List<string> symbols)
    {
        try
        {
            var webhook = _configuration["N8N:SymbolScannerWebhook"];
            var url = $"{_baseUrl}{webhook}";

            var payload = new
            {
                timestamp = DateTime.UtcNow,
                symbols
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending symbol update to N8N");
            return false;
        }
    }
}
