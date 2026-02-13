using BitgetApi;
using BitgetApi.TradingEngine.Models;
using BitgetApi.TradingEngine.Models.N8N;
using BitgetApi.TradingEngine.Strategies;
using BitgetApi.TradingEngine.Trading;
using BitgetApi.TradingEngine.N8N;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace BitgetApi.TradingEngine.Services;

public class StrategyOrchestrator
{
    private readonly List<IStrategy> _strategies;
    private readonly N8NWebhookClient _n8nClient;
    private readonly PositionManager _positionManager;
    private readonly RiskManager _riskManager;  
    private readonly PerformanceTracker _performanceTracker;
    private readonly SymbolManager _symbolManager;
    private readonly BitgetApiClient _bitgetClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StrategyOrchestrator> _logger;
    private readonly BitgetMarketDataService _marketDataService;

    public StrategyOrchestrator(
        IEnumerable<IStrategy> strategies,
        N8NWebhookClient n8nClient,
        PositionManager positionManager,
        RiskManager riskManager,
        PerformanceTracker performanceTracker,
        SymbolManager symbolManager,
        BitgetApiClient bitgetClient,
        IConfiguration configuration,
        ILogger<StrategyOrchestrator> logger,
        BitgetMarketDataService marketDataService)
    {
        _strategies = strategies.ToList();
        _n8nClient = n8nClient;
        _positionManager = positionManager;
        _riskManager = riskManager;
        _performanceTracker = performanceTracker;
        _symbolManager = symbolManager;
        _bitgetClient = bitgetClient;
        _configuration = configuration;
        _logger = logger;
        _marketDataService = marketDataService;

        Console.WriteLine($"🔍 [StrategyOrchestrator] Initialized with {_strategies.Count} strategies:");
        foreach (var strat in _strategies)
        {
            Console.WriteLine($"   - {strat.Name}: IsEnabled={strat.IsEnabled}");
        }
        
        // ✅ ADD VERIFICATION
        if (_strategies.Count == 0)
        {
            Console.WriteLine($"⚠️ WARNING: No strategies injected! Check Program.cs registration!");
        }
        
        var enabledCount = _strategies.Count(s => s.IsEnabled);
        if (enabledCount == 0)
        {
            Console.WriteLine($"⚠️ WARNING: All strategies are DISABLED! Check constructor defaults!");
        }
    }

    public async Task RunAnalysisAsync()
    {
        try
        {
            var symbols = _symbolManager.GetActiveSymbols();
            _logger.LogInformation("Starting analysis for {Count} symbols", symbols.Count);

            foreach (var symbol in symbols)
            {
                await AnalyzeSymbolWithN8NAsync(symbol);
                await Task.Delay(1000); // Rate limiting
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running analysis");
        }
    }

    /// <summary>
    /// Analyze a symbol with all enabled strategies and return signals
    /// </summary>
    public async Task<List<Signal>> AnalyzeSymbolAsync(string symbol)
    {
        _logger.LogInformation("📊 Analyzing {Symbol}...", symbol);

        var signals = new List<Signal>();

        try
        {
            // Fetch candles
            var candles = await _marketDataService.GetCandlesAsync(symbol, "1h", 200);

            // ✅ DEBUG LOGGING
            Console.WriteLine($"🔍 DEBUG: Received {candles?.Count ?? 0} candles for {symbol}");

            if (candles == null)
            {
                _logger.LogWarning("⚠️ Candles is NULL for {Symbol}", symbol);
                return signals;
            }

            if (candles.Count == 0)
            {
                _logger.LogWarning("⚠️ Candles list is EMPTY for {Symbol}", symbol);
                return signals;
            }

            if (candles.Count < 50)
            {
                _logger.LogWarning("⚠️ Insufficient candle data for {Symbol}: {Count} candles (need 50)",
                    symbol, candles.Count);
                return signals;
            }

            _logger.LogInformation("✅ Fetched {Count} candles for {Symbol}", candles.Count, symbol);

            // ✅ ENHANCED DEBUG LOGGING
            Console.WriteLine($"🔍 DEBUG: Total strategies: {_strategies.Count}");
            Console.WriteLine($"🔍 DEBUG: Enabled strategies: {_strategies.Count(s => s.IsEnabled)}");
            
            // ✅ ADD DETAILED STRATEGY INSPECTION
            Console.WriteLine($"🔍 DEBUG: Detailed strategy list:");
            foreach (var s in _strategies)
            {
                Console.WriteLine($"   - Name={s.Name}, IsEnabled={s.IsEnabled}, Type={s.GetType().FullName}");
            }

            // ✅ CREATE FILTERED LIST EXPLICITLY
            var enabledStrategies = _strategies.Where(s => s.IsEnabled).ToList();
            Console.WriteLine($"🔍 DEBUG: After .Where() filter: {enabledStrategies.Count} strategies");

            if (enabledStrategies.Count == 0)
            {
                Console.WriteLine($"⚠️ WARNING: No enabled strategies after filter! This is a LINQ bug!");
                Console.WriteLine($"⚠️ Falling back to manual filter...");
                
                // Manual filter fallback
                enabledStrategies = new List<IStrategy>();
                foreach (var s in _strategies)
                {
                    if (s.IsEnabled)
                    {
                        enabledStrategies.Add(s);
                        Console.WriteLine($"   ✅ Manually added: {s.Name}");
                    }
                }
            }

            // ✅ USE EXPLICIT LIST IN LOOP
            foreach (var strategy in enabledStrategies)
            {
                Console.WriteLine($"🚀 Running strategy: {strategy.Name}");
                
                try
                {
                    var signal = await strategy.GenerateSignalAsync(symbol, candles);

                    if (signal != null)
                    {
                        signals.Add(signal);
                        _logger.LogInformation("   ✅ {Strategy}: {Type} at ${Price} (confidence: {Confidence:F1}%)",
                            strategy.Name, signal.Type, signal.EntryPrice, signal.Confidence);
                    }
                    else
                    {
                        Console.WriteLine($"   ℹ️ {strategy.Name}: No signal");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in strategy {Strategy} for {Symbol}", strategy.Name, symbol);
                }
            }

            if (!signals.Any())
            {
                _logger.LogDebug("ℹ️ No signals generated for {Symbol}", symbol);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error analyzing {Symbol}", symbol);
        }

        return signals;
    }

    private async Task AnalyzeSymbolWithN8NAsync(string symbol)
    {
        try
        {
            // Check if already at max positions for this symbol
            var maxPerSymbol = _configuration.GetValue<int>("Trading:MaxPositionsPerSymbol", 1);
            if (_positionManager.GetOpenPositionCountForSymbol(symbol) >= maxPerSymbol)
            {
                _logger.LogDebug("Skipping {Symbol}: max positions reached", symbol);
                return;
            }

            // Get signals
            var signals = await AnalyzeSymbolAsync(symbol);

            // If we have signals, send to N8N for decision
            if (signals.Count > 0)
            {
                var candles = await _marketDataService.GetCandlesAsync(symbol, "1h", 200);
                var currentPrice = candles.Last().Close;
                var volume24h = candles.TakeLast(24).Sum(c => c.Volume);

                var marketData = new Dictionary<string, object>
                {
                    { "price", currentPrice },
                    { "volume24h", volume24h }
                };

                var decision = await _n8nClient.SendStrategyAnalysisAsync(symbol, signals, marketData);

                // Note: Actual trade execution happens when N8N calls back via /api/n8n/decision endpoint
                // This is handled by the N8NAgentController
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing {Symbol} with N8N", symbol);
        }
    }

    public async Task ExecuteTradeAsync(AgentDecision decision)
    {
        try
        {
            if (!_configuration.GetValue<bool>("Trading:EnableAutoTrading", false))
            {
                _logger.LogWarning("Auto-trading is disabled. Skipping trade execution for {Symbol}", decision.Symbol);
                return;
            }

            if (decision.Trade == null)
            {
                _logger.LogWarning("No trade details in decision for {Symbol}", decision.Symbol);
                return;
            }

            // Convert AgentDecision to Signal for validation
            var signal = new Signal
            {
                Symbol = decision.Symbol,
                Strategy = "N8N_Aggregated",
                Type = decision.Trade.Direction == "LONG" ? SignalType.LONG : SignalType.SHORT,
                EntryPrice = decision.Trade.EntryPrice,
                StopLoss = decision.Trade.StopLoss,
                TakeProfit = decision.Trade.TakeProfit,
                Confidence = decision.Trade.Confidence,
                Reason = decision.Reasoning
            };

            // Validate signal
            var maxPositions = _configuration.GetValue<int>("Trading:MaxTotalPositions", 8);
            var currentPositions = _positionManager.GetOpenPositionCount();

            if (!_riskManager.ValidateSignal(signal, currentPositions, maxPositions))
            {
                _logger.LogWarning("Signal validation failed for {Symbol}", decision.Symbol);
                return;
            }

            // Calculate position size with risk management
            var sizing = await _riskManager.CalculatePositionSizeAsync(decision);
            
            if (!sizing.HasValue)
            {
                _logger.LogWarning("Position sizing failed for {Symbol}", decision.Symbol);
                return;
            }

            // Execute the trade
            var position = await _positionManager.OpenFuturesPositionAsync(
                signal, 
                sizing.Value.positionSize, 
                sizing.Value.leverage);

            if (position != null)
            {
                await _performanceTracker.SavePositionAsync(position);
                
                _logger.LogInformation("Trade executed successfully: {Symbol} {Side} ${Size} with {Leverage}x leverage", 
                    decision.Symbol, decision.Trade.Direction, sizing.Value.positionSize, sizing.Value.leverage);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing trade for {Symbol}", decision.Symbol);
        }
    }

    public void ApplyParameterOptimization(ParameterOptimization optimization)
    {
        try
        {
            var strategy = _strategies.FirstOrDefault(s => s.Name == optimization.Strategy);
            
            if (strategy == null)
            {
                _logger.LogWarning("Strategy not found: {Strategy}", optimization.Strategy);
                return;
            }

            if (optimization.SuggestedValue == null)
            {
                _logger.LogWarning("No suggested value for optimization");
                return;
            }

            var newParameters = new Dictionary<string, object>(strategy.Parameters)
            {
                [optimization.Parameter] = optimization.SuggestedValue
            };

            strategy.UpdateParameters(newParameters);

            _logger.LogInformation("Applied parameter optimization: {Strategy}.{Parameter} = {Value}", 
                optimization.Strategy, optimization.Parameter, optimization.SuggestedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying parameter optimization");
        }
    }

    public async Task SendPerformanceUpdateAsync()
    {
        try
        {
            var metrics = await _performanceTracker.GetAllStrategyMetricsAsync();
            var overallPerformance = await _performanceTracker.GetOverallPerformanceAsync();

            // Update strategy metrics with current parameters
            foreach (var metric in metrics)
            {
                var strategy = _strategies.FirstOrDefault(s => s.Name == metric.StrategyName);
                if (strategy != null)
                {
                    metric.CurrentParameters = new Dictionary<string, object>(strategy.Parameters);
                }
            }

            await _n8nClient.SendPerformanceMetricsAsync(metrics, overallPerformance);

            _logger.LogInformation("Sent performance update to N8N");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending performance update");
        }
    }

    private async Task<List<Candle>> FetchCandleDataAsync(string symbol)
    {
        try
        {
            // Fetch 1-hour candles for the last 300 hours (~12.5 days)
            var endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var startTime = DateTimeOffset.UtcNow.AddHours(-300).ToUnixTimeMilliseconds();
            
            var response = await _bitgetClient.SpotMarket.GetCandlesticksAsync(
                symbol: symbol,
                granularity: "1h",
                startTime: startTime,
                endTime: endTime
            );

            if (response?.Code == "00000" && response.Data != null)
            {
                var candles = response.Data.Select(c => new Candle
                {
                    Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(c.Timestamp).DateTime,
                    Open = decimal.Parse(c.Open, CultureInfo.InvariantCulture),
                    High = decimal.Parse(c.High, CultureInfo.InvariantCulture),
                    Low = decimal.Parse(c.Low, CultureInfo.InvariantCulture),
                    Close = decimal.Parse(c.Close, CultureInfo.InvariantCulture),
                    Volume = decimal.Parse(c.BaseVolume, CultureInfo.InvariantCulture),
                    Symbol = symbol
                }).OrderBy(c => c.Timestamp).ToList();

                return candles;
            }

            _logger.LogWarning("Failed to fetch candle data for {Symbol}", symbol);
            return new List<Candle>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching candle data for {Symbol}", symbol);
            return new List<Candle>();
        }
    }
}
