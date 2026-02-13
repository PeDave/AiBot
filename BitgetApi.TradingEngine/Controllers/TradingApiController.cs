using Microsoft.AspNetCore.Mvc;
using BitgetApi.TradingEngine.Services;
using BitgetApi.TradingEngine.Trading;

namespace BitgetApi.TradingEngine.Controllers;

[ApiController]
[Route("api")]
public class TradingApiController : ControllerBase
{
    private readonly SymbolScanner _symbolScanner;
    private readonly StrategyOrchestrator _orchestrator;
    private readonly PerformanceTracker _performanceTracker;
    private readonly PositionManager _positionManager;
    private readonly BitgetMarketDataService _marketDataService;
    
    public TradingApiController(
        SymbolScanner symbolScanner,
        StrategyOrchestrator orchestrator,
        PerformanceTracker performanceTracker,
        PositionManager positionManager,
        BitgetMarketDataService marketDataService)
    {
        _symbolScanner = symbolScanner;
        _orchestrator = orchestrator;
        _performanceTracker = performanceTracker;
        _positionManager = positionManager;
        _marketDataService = marketDataService;
    }
    
    // ════════════════════════════════════════════════════════
    // SYMBOL SCANNING
    // ════════════════════════════════════════════════════════
    
    /// <summary>
    /// Scan top symbols by volume and volatility from Bitget
    /// </summary>
    [HttpGet("symbols/scan")]
    public async Task<IActionResult> ScanSymbols([FromQuery] int count = 10)
    {
        try
        {
            var symbols = await _symbolScanner.ScanTopSymbolsAsync(count);
            
            return Ok(new
            {
                symbols = symbols.Select(s => new
                {
                    symbol = s.Symbol,
                    score = s.Score,
                    volume24h = s.Volume,
                    priceChange24h = s.PriceChange,
                    price = s.Price,
                    high24h = s.High24h,
                    low24h = s.Low24h
                }),
                count = symbols.Count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    // ════════════════════════════════════════════════════════
    // STRATEGY ANALYSIS
    // ════════════════════════════════════════════════════════
    
    /// <summary>
    /// Analyze multiple symbols with all enabled strategies
    /// </summary>
    [HttpPost("strategies/analyze")]
    public async Task<IActionResult> AnalyzeStrategies([FromBody] AnalysisRequest request)
    {
        if (request.Symbols == null || !request.Symbols.Any())
            return BadRequest(new { error = "No symbols provided" });
        
        try
        {
            var results = new List<object>();
            
            foreach (var symbol in request.Symbols)
            {
                var signals = await _orchestrator.AnalyzeSymbolAsync(symbol);
                
                if (signals.Any())
                {
                    results.Add(new
                    {
                        symbol = symbol,
                        signals = signals.Select(s => new
                        {
                            strategy = s.Strategy,
                            type = s.Type.ToString(),
                            entryPrice = s.EntryPrice,
                            stopLoss = s.StopLoss,
                            takeProfit = s.TakeProfit,
                            confidence = s.Confidence,
                            reason = s.Reason,
                            timestamp = s.Timestamp
                        })
                    });
                }
            }
            
            return Ok(new { analyses = results, timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    // ════════════════════════════════════════════════════════
    // PERFORMANCE DATA
    // ════════════════════════════════════════════════════════
    
    /// <summary>
    /// Get performance metrics for all strategies
    /// </summary>
    [HttpGet("performance")]
    public async Task<IActionResult> GetPerformance()
    {
        try
        {
            var metrics = await _performanceTracker.GetAllMetricsAsync();
            
            return Ok(new
            {
                strategies = metrics.Select(m => new
                {
                    name = m.StrategyName,
                    winRate = m.WinRate,
                    roi = m.Roi,
                    drawdown = m.MaxDrawdown,
                    totalTrades = m.TotalTrades,
                    avgWin = m.AverageWinPercent,
                    avgLoss = m.AverageLossPercent,
                    parameters = m.CurrentParameters
                }),
                overallPerformance = new
                {
                    totalRoi = metrics.Sum(m => m.Roi),
                    totalTrades = metrics.Sum(m => m.TotalTrades),
                    avgWinRate = metrics.Any() ? metrics.Average(m => m.WinRate) : 0,
                    maxDrawdown = metrics.Any() ? metrics.Max(m => m.MaxDrawdown) : 0
                },
                openPositions = await _positionManager.GetOpenPositionsCountAsync(),
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    // ════════════════════════════════════════════════════════
    // MARKET DATA (for N8N to fetch chart data if needed)
    // ════════════════════════════════════════════════════════
    
    /// <summary>
    /// Get candle data for a symbol
    /// </summary>
    [HttpGet("market/{symbol}/candles")]
    public async Task<IActionResult> GetCandles(
        string symbol,
        [FromQuery] string timeframe = "1h",
        [FromQuery] int limit = 200)
    {
        try
        {
            var candles = await _marketDataService.GetCandlesAsync(symbol, timeframe, limit);
            
            return Ok(new
            {
                symbol = symbol,
                timeframe = timeframe,
                candles = candles.Select(c => new
                {
                    timestamp = c.Timestamp,
                    open = c.Open,
                    high = c.High,
                    low = c.Low,
                    close = c.Close,
                    volume = c.Volume
                }),
                count = candles.Count
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Get current ticker data for a symbol
    /// </summary>
    [HttpGet("market/{symbol}/ticker")]
    public async Task<IActionResult> GetTicker(string symbol)
    {
        try
        {
            var ticker = await _marketDataService.GetTickerAsync(symbol);
            
            return Ok(new
            {
                symbol = ticker.Symbol,
                price = ticker.LastPrice,
                volume24h = ticker.QuoteVolume,
                priceChange24h = ticker.ChangeUtc,
                high24h = ticker.High24h,
                low24h = ticker.Low24h,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}

// ════════════════════════════════════════════════════════
// REQUEST MODELS
// ════════════════════════════════════════════════════════

public class AnalysisRequest
{
    public List<string> Symbols { get; set; } = new();
}
