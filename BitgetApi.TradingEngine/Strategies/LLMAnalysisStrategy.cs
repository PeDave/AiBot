using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BitgetApi.TradingEngine.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BitgetApi.TradingEngine.Strategies;

public class LLMAnalysisStrategy : IStrategy
{
    private readonly ILogger<LLMAnalysisStrategy> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    
    public string Name => "LLM_Analysis";
    public string Market => "Futures";
    public bool IsEnabled { get; set; }
    public Dictionary<string, object> Parameters { get; set; }

    public LLMAnalysisStrategy(
        ILogger<LLMAnalysisStrategy> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
        IsEnabled = configuration.GetValue<bool>("Strategies:LLMAnalysis:Enabled", true);
        Parameters = configuration.GetSection("Strategies:LLMAnalysis:Parameters").Get<Dictionary<string, object>>() 
                    ?? new Dictionary<string, object>();
    }

    public async Task<Signal?> GenerateSignalAsync(string symbol, List<Candle> candles)
    {
        try
        {
            _logger.LogInformation("🤖 [LLM_Analysis] Analyzing {Symbol} with GPT-4", symbol);

            if (candles.Count < 50)
            {
                _logger.LogWarning("⚠️ [LLM_Analysis] Not enough candles ({Count})", candles.Count);
                return null;
            }

            // Prepare chart data summary
            var currentCandle = candles.Last();
            var recentCandles = candles.TakeLast(20).ToList();
            
            var high = recentCandles.Max(c => c.High);
            var low = recentCandles.Min(c => c.Low);
            var avgVolume = recentCandles.Average(c => c.Volume);
            
            // Calculate RSI for context
            var rsi = CalculateRSI(candles, 14);

            // Build prompt
            var prompt = BuildAnalysisPrompt(symbol, currentCandle, high, low, avgVolume, rsi, recentCandles);

            // Call OpenAI API
            var result = await CallOpenAIAsync(prompt);

            if (result == null)
            {
                _logger.LogWarning("⚠️ [LLM_Analysis] No response from LLM");
                return null;
            }

            _logger.LogInformation("🤖 [LLM_Analysis] {Symbol}: {Direction} (confidence: {Confidence}%)", 
                symbol, result.Direction, result.Confidence);
            _logger.LogInformation("💭 Reasoning: {Reasoning}", result.Reasoning);

            if (result.Direction == "NO_ACTION")
            {
                return null;
            }

            var signalType = result.Direction == "LONG" ? SignalType.LONG : SignalType.SHORT;
            var entryPrice = currentCandle.Close;
            var stopLoss = signalType == SignalType.LONG 
                ? entryPrice * 0.97m 
                : entryPrice * 1.03m;
            var takeProfit = signalType == SignalType.LONG 
                ? entryPrice * 1.06m 
                : entryPrice * 0.94m;

            return new Signal
            {
                Symbol = symbol,
                Strategy = Name,
                Type = signalType,
                EntryPrice = entryPrice,
                StopLoss = stopLoss,
                TakeProfit = takeProfit,
                Confidence = (double)result.Confidence,
                Reason = result.Reasoning,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [LLM_Analysis] Error analyzing {Symbol}", symbol);
            return null;
        }
    }

    public void UpdateParameters(Dictionary<string, object> newParameters)
    {
        Parameters = newParameters;
    }

    private string BuildAnalysisPrompt(string symbol, Candle current, decimal high, decimal low, 
        decimal avgVolume, decimal rsi, List<Candle> recentCandles)
    {
        var priceData = string.Join("\n", recentCandles.Select((c, i) => 
            $"{i + 1}. Open: {c.Open:F2}, High: {c.High:F2}, Low: {c.Low:F2}, Close: {c.Close:F2}, Vol: {c.Volume:F0}"));

        return $@"You are an expert cryptocurrency trader analyzing {symbol}.

**Current Market Data:**
- Current Price: ${current.Close:F2}
- 20-period High: ${high:F2}
- 20-period Low: ${low:F2}
- RSI(14): {rsi:F2}
- Current Volume: {current.Volume:F0} (avg: {avgVolume:F0})

**Recent 20 Candles (hourly):**
{priceData}

**Task:**
Analyze this data and provide:
1. Pattern recognition (head & shoulders, double top/bottom, triangles, etc.)
2. Trend analysis (bullish/bearish/neutral)
3. Trading recommendation: LONG, SHORT, or NO_ACTION
4. Confidence level: 0-100%
5. Brief reasoning (2-3 sentences)

**Response Format (JSON only):**
{{
  ""direction"": ""LONG"" | ""SHORT"" | ""NO_ACTION"",
  ""confidence"": 75,
  ""reasoning"": ""Strong bullish pattern with high volume confirmation. RSI shows momentum...""
}}

Respond with ONLY valid JSON, no additional text.";
    }

    private async Task<LLMAnalysisResult?> CallOpenAIAsync(string prompt)
    {
        try
        {
            var apiKey = _configuration["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("❌ OpenAI API key not configured");
                return null;
            }

            var model = _configuration.GetValue<string>("OpenAI:Model", "gpt-4-turbo-preview");
            var maxTokens = _configuration.GetValue<int>("OpenAI:MaxTokens", 500);
            var temperature = _configuration.GetValue<double>("OpenAI:Temperature", 0.3);

            var requestBody = new
            {
                model,
                messages = new[]
                {
                    new { role = "system", content = "You are a professional cryptocurrency trading analyst. Respond only with valid JSON." },
                    new { role = "user", content = prompt }
                },
                max_tokens = maxTokens,
                temperature
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody), 
                Encoding.UTF8, 
                "application/json");

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenAI API error: {StatusCode} - {Error}", response.StatusCode, errorContent);
                response.EnsureSuccessStatusCode();
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            
            OpenAIResponse? openAIResponse;
            try
            {
                openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseJson);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize OpenAI API response. Response: {Response}", responseJson);
                return null;
            }

            var content = openAIResponse?.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrEmpty(content))
            {
                _logger.LogWarning("OpenAI API returned empty content");
                return null;
            }

            // Parse LLM response
            try
            {
                var result = JsonSerializer.Deserialize<LLMAnalysisResult>(content.Trim());
                return result;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse LLM response as JSON. Content: {Content}", content);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling OpenAI API");
            return null;
        }
    }

    private decimal CalculateRSI(List<Candle> candles, int period)
    {
        if (candles.Count < period + 1) return 50;

        var recentCandles = candles.TakeLast(period + 1).ToList();
        decimal gainSum = 0;
        decimal lossSum = 0;

        for (int i = 1; i < recentCandles.Count; i++)
        {
            var change = recentCandles[i].Close - recentCandles[i - 1].Close;
            if (change > 0) gainSum += change;
            else lossSum += Math.Abs(change);
        }

        if (lossSum == 0) return 100;

        var avgGain = gainSum / period;
        var avgLoss = lossSum / period;
        var rs = avgGain / avgLoss;
        var rsi = 100 - (100 / (1 + rs));

        return rsi;
    }
}

// Models
public class LLMAnalysisResult
{
    public string Direction { get; set; } = "NO_ACTION";
    public decimal Confidence { get; set; }
    public string Reasoning { get; set; } = "";
}

public class OpenAIResponse
{
    public List<OpenAIChoice>? Choices { get; set; }
}

public class OpenAIChoice
{
    public OpenAIMessage? Message { get; set; }
}

public class OpenAIMessage
{
    public string? Content { get; set; }
}
