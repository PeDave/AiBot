using BitgetApi.TradingEngine.Models;
using BitgetApi.TradingEngine.Models.N8N;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BitgetApi.TradingEngine.Trading;

public class RiskManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RiskManager> _logger;
    private readonly BitgetFuturesClient _futuresClient;

    public RiskManager(
        IConfiguration configuration,
        ILogger<RiskManager> logger,
        BitgetFuturesClient futuresClient)
    {
        _configuration = configuration;
        _logger = logger;
        _futuresClient = futuresClient;
    }

    public async Task<(decimal positionSize, int leverage)?> CalculatePositionSizeAsync(AgentDecision decision)
    {
        try
        {
            if (decision.Trade == null)
                return null;

            var accountBalance = await _futuresClient.GetAccountBalanceAsync();
            
            if (accountBalance <= 0)
            {
                _logger.LogWarning("Insufficient account balance");
                return null;
            }

            var positionSize = decision.Trade.PositionSizeUsd;
            var leverage = decision.Trade.Leverage;

            // Apply risk limits
            var maxPositionPercent = _configuration.GetValue<double>("Trading:MaxPositionSizePercent", 5.0);
            var maxPositionSize = accountBalance * (decimal)(maxPositionPercent / 100);

            if (positionSize > maxPositionSize)
            {
                _logger.LogWarning("Position size ${Size} exceeds max ${Max}, adjusting", positionSize, maxPositionSize);
                positionSize = maxPositionSize;
            }

            // Adjust leverage based on confidence
            var adjustedLeverage = AdjustLeverageByConfidence(leverage, decision.Trade.Confidence);

            _logger.LogInformation("Position sizing: ${Size} with {Leverage}x leverage (confidence: {Confidence}%)", 
                positionSize, adjustedLeverage, decision.Trade.Confidence);

            return (positionSize, adjustedLeverage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating position size");
            return null;
        }
    }

    public bool ValidateSignal(Signal signal, int currentOpenPositions, int maxPositions)
    {
        // Check confidence threshold
        var minConfidence = _configuration.GetValue<double>("Trading:MinConfidenceThreshold", 70.0);
        if (signal.Confidence < minConfidence)
        {
            _logger.LogDebug("Signal rejected: confidence {Confidence} below threshold {Threshold}", 
                signal.Confidence, minConfidence);
            return false;
        }

        // Check position limits
        if (currentOpenPositions >= maxPositions)
        {
            _logger.LogDebug("Signal rejected: max positions reached ({Count}/{Max})", 
                currentOpenPositions, maxPositions);
            return false;
        }

        // Validate stop loss and take profit
        if (signal.Type == SignalType.LONG)
        {
            if (signal.StopLoss >= signal.EntryPrice)
            {
                _logger.LogWarning("Invalid LONG signal: SL {SL} >= Entry {Entry}", signal.StopLoss, signal.EntryPrice);
                return false;
            }
            if (signal.TakeProfit <= signal.EntryPrice)
            {
                _logger.LogWarning("Invalid LONG signal: TP {TP} <= Entry {Entry}", signal.TakeProfit, signal.EntryPrice);
                return false;
            }
        }
        else if (signal.Type == SignalType.SHORT)
        {
            if (signal.StopLoss <= signal.EntryPrice)
            {
                _logger.LogWarning("Invalid SHORT signal: SL {SL} <= Entry {Entry}", signal.StopLoss, signal.EntryPrice);
                return false;
            }
            if (signal.TakeProfit >= signal.EntryPrice)
            {
                _logger.LogWarning("Invalid SHORT signal: TP {TP} >= Entry {Entry}", signal.TakeProfit, signal.EntryPrice);
                return false;
            }
        }

        return true;
    }

    public decimal CalculateDcaAmount(Signal signal)
    {
        var baseAmount = _configuration.GetValue<double>("Trading:BaseDcaAmountUsd", 10.0);
        
        if (signal.Metadata.TryGetValue("dca_amount_usd", out var amountObj) && amountObj is double amount)
        {
            return (decimal)amount;
        }

        return (decimal)baseAmount;
    }

    private int AdjustLeverageByConfidence(int baseLeverage, double confidence)
    {
        // Reduce leverage for lower confidence
        if (confidence < 75)
        {
            return Math.Max(1, baseLeverage / 2);
        }
        else if (confidence < 85)
        {
            return Math.Max(1, baseLeverage * 3 / 4);
        }

        return baseLeverage;
    }

    public bool CheckDrawdownLimit(double currentDrawdown)
    {
        var maxDrawdown = _configuration.GetValue<double>("Trading:MaxDrawdownPercent", 20.0);
        
        if (Math.Abs(currentDrawdown) >= maxDrawdown)
        {
            _logger.LogWarning("Drawdown limit reached: {Current}% >= {Max}%", currentDrawdown, maxDrawdown);
            return false;
        }

        return true;
    }
}
