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
            _logger.LogWarning("⚠️ No strategies configured for the orchestrator!");
        }
        
        var enabledCount = _strategies.Count(s => s.IsEnabled);
        if (enabledCount == 0)
        {
            _logger.LogWarning("⚠️ All registered strategies are currently disabled!");
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
            _logger.LogDebug("🔍 DEBUG: Total strategies: {Count}", _strategies.Count);
            _logger.LogDebug("🔍 DEBUG: Enabled strategies: {Count}", _strategies.Count(s => s.IsEnabled));
            
            // ✅ ADD DETAILED STRATEGY INSPECTION
            _logger.LogDebug("🔍 DEBUG: Detailed strategy list:");
            foreach (var s in _strategies)
            {
                _logger.LogDebug("   - Name={Name}, IsEnabled={IsEnabled}, Type={Type}", s.Name, s.IsEnabled, s.GetType().FullName);
            }

            // ✅ CREATE FILTERED LIST EXPLICITLY (fixes the issue where LINQ query wasn't being executed)
            var enabledStrategies = _strategies.Where(s => s.IsEnabled).ToList();
            _logger.LogDebug("🔍 DEBUG: After .Where() filter: {Count} strategies", enabledStrategies.Count);

            if (enabledStrategies.Count == 0)
            {
                _logger.LogWarning("⚠️ No enabled strategies found after filtering!");
                return signals;
            }

            // ✅ USE EXPLICIT LIST IN LOOP
            foreach (var strategy in enabledStrategies)
            {
                _logger.LogInformation("🚀 Running strategy: {Strategy}", strategy.Name);
                
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
                        _logger.LogDebug("   ℹ️ {Strategy}: No signal for {Symbol}", strategy.Name, symbol);
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

            // Get signals from all strategies
            var signals = await AnalyzeSymbolAsync(symbol);

            if (signals.Count == 0)
            {
                _logger.LogDebug("No signals for {Symbol}", symbol);
                return;
            }

            _logger.LogInformation("📊 Found {Count} signals for {Symbol}", signals.Count, symbol);

            // ✅ DIRECT TRADING LOGIC - Aggregate signals and make decision
            var longSignals = signals.Where(s => s.Type == SignalType.LONG).ToList();
            var shortSignals = signals.Where(s => s.Type == SignalType.SHORT).ToList();

            _logger.LogInformation("   📈 LONG signals: {LongCount}, 📉 SHORT signals: {ShortCount}", 
                longSignals.Count, shortSignals.Count);

            // Decision criteria: At least 2 strategies agree AND average confidence > 60%
            var minAgreement = _configuration.GetValue<int>("Trading:MinStrategyAgreement", 2);
            var minConfidence = _configuration.GetValue<decimal>("Trading:MinConfidence", 60m);

            // Process LONG signals
            if (longSignals.Count >= minAgreement)
            {
                var avgConfidence = longSignals.Average(s => s.Confidence);
                var maxConfidence = longSignals.Max(s => s.Confidence);
                var bestSignal = longSignals.OrderByDescending(s => s.Confidence).First();

                _logger.LogInformation("   💡 LONG consensus: {Count} strategies, avg confidence: {AvgConf:F1}%, max: {MaxConf:F1}%",
                    longSignals.Count, avgConfidence, maxConfidence);

                if ((decimal)avgConfidence >= minConfidence)
                {
                    _logger.LogInformation("✅ TRADE DECISION: LONG on {Symbol}", symbol);
                    
                    // List all agreeing strategies
                    foreach (var sig in longSignals)
                    {
                        _logger.LogInformation("   - {Strategy}: {Confidence:F1}% - {Reason}", 
                            sig.Strategy, sig.Confidence, sig.Reason);
                    }

                    // Fetch current market data
                    var candles = await _marketDataService.GetCandlesAsync(symbol, "1h", 200);
                    var currentPrice = candles.Last().Close;

                    // Create trade decision
                    var decision = new AgentDecision
                    {
                        Symbol = symbol,
                        Decision = "EXECUTE",
                        Trade = new TradeDecision
                        {
                            Direction = "LONG",
                            EntryPrice = currentPrice,
                            StopLoss = bestSignal.StopLoss,
                            TakeProfit = bestSignal.TakeProfit,
                            PositionSizeUsd = _configuration.GetValue<decimal>("Trading:DefaultPositionSizeUsd", 100m),
                            Leverage = _configuration.GetValue<int>("Trading:DefaultLeverage", 5),
                            Confidence = avgConfidence
                        },
                        StrategyScores = longSignals.ToDictionary(s => s.Strategy, s => s.Confidence),
                        Reasoning = $"{longSignals.Count}/{signals.Count} strategies agree on LONG. " +
                                   $"Average confidence: {avgConfidence:F1}%. " +
                                   $"Strategies: {string.Join(", ", longSignals.Select(s => s.Strategy))}"
                    };

                    _logger.LogInformation("🚀 Executing LONG trade: ${Size} at ${Price}, SL: ${SL}, TP: ${TP}, Leverage: {Lev}x",
                        decision.Trade.PositionSizeUsd,
                        decision.Trade.EntryPrice,
                        decision.Trade.StopLoss,
                        decision.Trade.TakeProfit,
                        decision.Trade.Leverage);

                    await ExecuteTradeAsync(decision);
                    return;
                }
                else
                {
                    _logger.LogInformation("⚠️ LONG consensus found but confidence too low ({AvgConf:F1}% < {MinConf}%)",
                        avgConfidence, minConfidence);
                }
            }

            // Process SHORT signals
            if (shortSignals.Count >= minAgreement)
            {
                var avgConfidence = shortSignals.Average(s => s.Confidence);
                var maxConfidence = shortSignals.Max(s => s.Confidence);
                var bestSignal = shortSignals.OrderByDescending(s => s.Confidence).First();

                _logger.LogInformation("   💡 SHORT consensus: {Count} strategies, avg confidence: {AvgConf:F1}%, max: {MaxConf:F1}%",
                    shortSignals.Count, avgConfidence, maxConfidence);

                if ((decimal)avgConfidence >= minConfidence)
                {
                    _logger.LogInformation("✅ TRADE DECISION: SHORT on {Symbol}", symbol);
                    
                    // List all agreeing strategies
                    foreach (var sig in shortSignals)
                    {
                        _logger.LogInformation("   - {Strategy}: {Confidence:F1}% - {Reason}", 
                            sig.Strategy, sig.Confidence, sig.Reason);
                    }

                    // Fetch current market data
                    var candles = await _marketDataService.GetCandlesAsync(symbol, "1h", 200);
                    var currentPrice = candles.Last().Close;

                    // Create trade decision
                    var decision = new AgentDecision
                    {
                        Symbol = symbol,
                        Decision = "EXECUTE",
                        Trade = new TradeDecision
                        {
                            Direction = "SHORT",
                            EntryPrice = currentPrice,
                            StopLoss = bestSignal.StopLoss,
                            TakeProfit = bestSignal.TakeProfit,
                            PositionSizeUsd = _configuration.GetValue<decimal>("Trading:DefaultPositionSizeUsd", 100m),
                            Leverage = _configuration.GetValue<int>("Trading:DefaultLeverage", 5),
                            Confidence = avgConfidence
                        },
                        StrategyScores = shortSignals.ToDictionary(s => s.Strategy, s => s.Confidence),
                        Reasoning = $"{shortSignals.Count}/{signals.Count} strategies agree on SHORT. " +
                                   $"Average confidence: {avgConfidence:F1}%. " +
                                   $"Strategies: {string.Join(", ", shortSignals.Select(s => s.Strategy))}"
                    };

                    _logger.LogInformation("🚀 Executing SHORT trade: ${Size} at ${Price}, SL: ${SL}, TP: ${TP}, Leverage: {Lev}x",
                        decision.Trade.PositionSizeUsd,
                        decision.Trade.EntryPrice,
                        decision.Trade.StopLoss,
                        decision.Trade.TakeProfit,
                        decision.Trade.Leverage);

                    await ExecuteTradeAsync(decision);
                    return;
                }
                else
                {
                    _logger.LogInformation("⚠️ SHORT consensus found but confidence too low ({AvgConf:F1}% < {MinConf}%)",
                        avgConfidence, minConfidence);
                }
            }

            // No consensus
            if (longSignals.Count > 0 || shortSignals.Count > 0)
            {
                _logger.LogInformation("ℹ️ No consensus: {LongCount} LONG vs {ShortCount} SHORT (need {MinAgreement}+ agreement)",
                    longSignals.Count, shortSignals.Count, minAgreement);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing {Symbol}", symbol);
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
                
                _logger.LogInformation("✅ Trade executed successfully: {Symbol} {Side} ${Size} with {Leverage}x leverage", 
                    decision.Symbol, decision.Trade.Direction, sizing.Value.positionSize, sizing.Value.leverage);
            }
            else
            {
                _logger.LogWarning("⚠️ Trade execution failed for {Symbol}", decision.Symbol);
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
