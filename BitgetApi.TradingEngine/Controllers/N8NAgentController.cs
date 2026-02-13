using Microsoft.AspNetCore.Mvc;
using BitgetApi.TradingEngine.Models.N8N;
using BitgetApi.TradingEngine.Services;
using Microsoft.Extensions.Logging;

namespace BitgetApi.TradingEngine.Controllers;

[ApiController]
[Route("api/n8n")]
public class N8NAgentController : ControllerBase
{
    private readonly SymbolManager _symbolManager;
    private readonly StrategyOrchestrator _orchestrator;
    private readonly ILogger<N8NAgentController> _logger;

    public N8NAgentController(
        SymbolManager symbolManager,
        StrategyOrchestrator orchestrator,
        ILogger<N8NAgentController> logger)
    {
        _symbolManager = symbolManager;
        _orchestrator = orchestrator;
        _logger = logger;
    }

    /// <summary>
    /// Receives symbol recommendations from N8N
    /// </summary>
    [HttpPost("symbols")]
    public IActionResult ReceiveSymbols([FromBody] SymbolRecommendation recommendation)
    {
        try
        {
            if (recommendation?.SelectedSymbols == null || recommendation.SelectedSymbols.Count == 0)
            {
                return BadRequest("No symbols provided");
            }

            _symbolManager.UpdateSymbols(recommendation.SelectedSymbols, recommendation.Reasoning);
            
            _logger.LogInformation("Updated symbols from N8N: {Count} symbols", recommendation.SelectedSymbols.Count);

            return Ok(new
            {
                success = true,
                message = $"Updated {recommendation.SelectedSymbols.Count} symbols",
                symbols = recommendation.SelectedSymbols
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving symbols from N8N");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Receives trade decision from N8N
    /// </summary>
    [HttpPost("decision")]
    public async Task<IActionResult> ReceiveDecision([FromBody] AgentDecision decision)
    {
        try
        {
            if (decision == null)
            {
                return BadRequest("No decision provided");
            }

            _logger.LogInformation("Received decision from N8N for {Symbol}: {Decision}", 
                decision.Symbol, decision.Decision);

            if (decision.Decision == "EXECUTE" && decision.Trade != null)
            {
                await _orchestrator.ExecuteTradeAsync(decision);
                
                return Ok(new
                {
                    success = true,
                    message = $"Trade executed for {decision.Symbol}",
                    decision = decision.Decision,
                    confidence = decision.Trade.Confidence
                });
            }

            return Ok(new
            {
                success = true,
                message = $"No action taken for {decision.Symbol}",
                decision = decision.Decision,
                reasoning = decision.Reasoning
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing decision from N8N");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Receives optimization recommendations from N8N
    /// </summary>
    [HttpPost("optimize")]
    public IActionResult ReceiveOptimization([FromBody] OptimizationRecommendation optimization)
    {
        try
        {
            if (optimization?.Optimizations == null || optimization.Optimizations.Count == 0)
            {
                return Ok(new { success = true, message = "No optimizations to apply" });
            }

            var appliedCount = 0;
            
            foreach (var opt in optimization.Optimizations)
            {
                if (opt.Confidence >= 0.7) // Only apply high-confidence optimizations
                {
                    _orchestrator.ApplyParameterOptimization(opt);
                    appliedCount++;
                    
                    _logger.LogInformation("Applied optimization for {Strategy}.{Parameter}: {CurrentValue} -> {SuggestedValue}", 
                        opt.Strategy, opt.Parameter, opt.CurrentValue, opt.SuggestedValue);
                }
            }

            return Ok(new
            {
                success = true,
                message = $"Applied {appliedCount} optimizations",
                totalRecommendations = optimization.Optimizations.Count,
                appliedCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving optimization from N8N");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}
