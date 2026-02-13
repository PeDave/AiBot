using BitgetApi.TradingEngine.Models;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace BitgetApi.TradingEngine.Services;

public class PerformanceTracker
{
    private readonly TradingDbContext _dbContext;
    private readonly ILogger<PerformanceTracker> _logger;

    public PerformanceTracker(TradingDbContext dbContext, ILogger<PerformanceTracker> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task SavePositionAsync(Position position)
    {
        try
        {
            _dbContext.Positions.Add(position);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Saved position to database: {Symbol} {Strategy}", position.Symbol, position.Strategy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving position to database");
        }
    }

    public async Task UpdatePositionAsync(Position position)
    {
        try
        {
            _dbContext.Positions.Update(position);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Updated position in database: {Symbol} {Strategy}", position.Symbol, position.Strategy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating position in database");
        }
    }

    public async Task SaveDcaOrderAsync(DcaOrder order)
    {
        try
        {
            _dbContext.DcaOrders.Add(order);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Saved DCA order to database: {Symbol} ${Amount}", order.Symbol, order.AmountUsd);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving DCA order to database");
        }
    }

    public async Task<StrategyMetrics> CalculateStrategyMetricsAsync(string strategyName)
    {
        try
        {
            var positions = await _dbContext.Positions
                .Where(p => p.Strategy == strategyName && p.Status == PositionStatus.Closed)
                .ToListAsync();

            if (positions.Count == 0)
            {
                return new StrategyMetrics
                {
                    StrategyName = strategyName,
                    TotalTrades = 0
                };
            }

            var winningTrades = positions.Count(p => p.PnL > 0);
            var losingTrades = positions.Count(p => p.PnL < 0);
            var winRate = (double)winningTrades / positions.Count * 100;
            
            var totalPnl = positions.Sum(p => p.PnL);
            var totalInvested = positions.Sum(p => p.EntryPrice * p.Size);
            var roi = totalInvested > 0 ? (double)(totalPnl / totalInvested * 100) : 0;

            var avgWinPercent = winningTrades > 0 
                ? (double)positions.Where(p => p.PnL > 0).Average(p => p.PnLPercent) 
                : 0;
            
            var avgLossPercent = losingTrades > 0 
                ? (double)positions.Where(p => p.PnL < 0).Average(p => p.PnLPercent) 
                : 0;

            // Calculate max drawdown
            var maxDrawdown = CalculateMaxDrawdown(positions);

            var metrics = new StrategyMetrics
            {
                StrategyName = strategyName,
                WinRate = winRate,
                Roi = roi,
                MaxDrawdown = maxDrawdown,
                TotalTrades = positions.Count,
                WinningTrades = winningTrades,
                LosingTrades = losingTrades,
                AverageWinPercent = avgWinPercent,
                AverageLossPercent = avgLossPercent
            };

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating metrics for {Strategy}", strategyName);
            return new StrategyMetrics { StrategyName = strategyName };
        }
    }

    public async Task<List<StrategyMetrics>> GetAllStrategyMetricsAsync()
    {
        try
        {
            var strategies = await _dbContext.Positions
                .Select(p => p.Strategy)
                .Distinct()
                .ToListAsync();

            var metrics = new List<StrategyMetrics>();
            
            foreach (var strategy in strategies)
            {
                var strategyMetrics = await CalculateStrategyMetricsAsync(strategy);
                metrics.Add(strategyMetrics);
            }

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all strategy metrics");
            return new List<StrategyMetrics>();
        }
    }

    public async Task<Dictionary<string, object>> GetOverallPerformanceAsync()
    {
        try
        {
            var allMetrics = await GetAllStrategyMetricsAsync();

            if (allMetrics.Count == 0)
            {
                return new Dictionary<string, object>
                {
                    { "totalRoi", 0.0 },
                    { "winRate", 0.0 },
                    { "maxDrawdown", 0.0 },
                    { "totalTrades", 0 }
                };
            }

            var totalTrades = allMetrics.Sum(m => m.TotalTrades);
            var totalWinningTrades = allMetrics.Sum(m => m.WinningTrades);
            var overallWinRate = totalTrades > 0 ? (double)totalWinningTrades / totalTrades * 100 : 0;
            
            var avgRoi = allMetrics.Average(m => m.Roi);
            var maxDrawdown = allMetrics.Min(m => m.MaxDrawdown);

            return new Dictionary<string, object>
            {
                { "totalRoi", avgRoi },
                { "winRate", overallWinRate },
                { "maxDrawdown", maxDrawdown },
                { "totalTrades", totalTrades }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting overall performance");
            return new Dictionary<string, object>();
        }
    }

    private double CalculateMaxDrawdown(List<Position> positions)
    {
        if (positions.Count == 0)
            return 0;

        var sortedPositions = positions.OrderBy(p => p.CloseTime).ToList();
        var runningPnl = 0m;
        var peak = 0m;
        var maxDrawdown = 0.0;

        foreach (var position in sortedPositions)
        {
            runningPnl += position.PnL;
            
            if (runningPnl > peak)
            {
                peak = runningPnl;
            }
            
            var drawdown = peak > 0 ? (double)((peak - runningPnl) / peak * 100) : 0;
            
            if (drawdown > maxDrawdown)
            {
                maxDrawdown = drawdown;
            }
        }

        return -maxDrawdown; // Return as negative value
    }

    public async Task<List<StrategyMetrics>> GetAllMetricsAsync()
    {
        return await GetAllStrategyMetricsAsync();
    }

    public async Task SaveMetricsSnapshotAsync(StrategyMetrics metrics)
    {
        try
        {
            _dbContext.StrategyMetrics.Add(metrics);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving metrics snapshot");
        }
    }
}
