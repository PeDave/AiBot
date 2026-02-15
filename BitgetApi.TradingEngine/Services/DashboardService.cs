using BitgetApi.TradingEngine.Models;
using Microsoft.EntityFrameworkCore;

namespace BitgetApi.TradingEngine.Services;

public class DashboardService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DashboardService> _logger;

    public decimal SpotBalance { get; private set; }
    public decimal EarnBalance { get; private set; }
    public decimal TotalBalance => SpotBalance + EarnBalance;

    public List<WatchlistItem> Watchlist { get; private set; } = new();
    public List<SignalItem> RecentSignals { get; private set; } = new();
    public List<DecisionItem> RecentDecisions { get; private set; } = new();
    public List<StrategyStatus> Strategies { get; private set; } = new();

    public bool IsN8NRunning { get; private set; }
    public int LastWebhookLatency { get; private set; }
    public int AnalysisInterval { get; private set; }
    public bool AutoTradingEnabled { get; private set; }

    public DashboardService(IServiceProvider serviceProvider, ILogger<DashboardService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task LoadDataAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            
            // Load portfolio data
            await LoadPortfolioAsync(scope);
            
            // Load watchlist
            await LoadWatchlistAsync(scope);
            
            // Load recent signals from database
            await LoadRecentSignalsAsync(scope);
            
            // Load recent decisions from database
            await LoadRecentDecisionsAsync(scope);
            
            // Load strategy status
            LoadStrategyStatus(scope);
            
            // Load system health
            LoadSystemHealth(scope);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
        }
    }

    private async Task LoadPortfolioAsync(IServiceScope scope)
    {
        // TODO: Implement Bitget API call to fetch balances
        // For now, use placeholder values
        SpotBalance = 0;
        EarnBalance = 0;
        await Task.CompletedTask;
    }

    private async Task LoadWatchlistAsync(IServiceScope scope)
    {
        var symbolManager = scope.ServiceProvider.GetRequiredService<SymbolManager>();
        
        Watchlist = new List<WatchlistItem>();
        
        foreach (var symbol in symbolManager.GetActiveSymbols())
        {
            // Fetch current price and 24h change
            // TODO: Implement actual market data fetching
            Watchlist.Add(new WatchlistItem
            {
                Symbol = symbol,
                Price = 0,
                Change24h = 0,
                LastSignal = "-"
            });
        }
        
        await Task.CompletedTask;
    }

    private async Task LoadRecentSignalsAsync(IServiceScope scope)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
        
        RecentSignals = await dbContext.TradeSignals
            .OrderByDescending(s => s.Timestamp)
            .Take(10)
            .Select(s => new SignalItem
            {
                Timestamp = s.Timestamp,
                Symbol = s.Symbol,
                Strategy = s.Strategy,
                Direction = s.Type,
                Confidence = s.Confidence
            })
            .ToListAsync();
    }

    private async Task LoadRecentDecisionsAsync(IServiceScope scope)
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
        
        RecentDecisions = await dbContext.N8NDecisions
            .OrderByDescending(d => d.Timestamp)
            .Take(10)
            .Select(d => new DecisionItem
            {
                Timestamp = d.Timestamp,
                Symbol = d.Symbol,
                Decision = d.Decision,
                Reasoning = d.Reasoning
            })
            .ToListAsync();
    }

    private void LoadStrategyStatus(IServiceScope scope)
    {
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        Strategies = new List<StrategyStatus>
        {
            new() 
            { 
                Name = "RSI_Volume_EMA", 
                IsEnabled = config.GetValue<bool>("Strategies:RSI_Volume_EMA:Enabled", false)
            },
            new() 
            { 
                Name = "FVG_Liquidity", 
                IsEnabled = config.GetValue<bool>("Strategies:FVG_Liquidity:Enabled", false)
            },
            new() 
            { 
                Name = "Swing", 
                IsEnabled = config.GetValue<bool>("Strategies:Swing:Enabled", false)
            },
            new() 
            { 
                Name = "Weekly_DCA", 
                IsEnabled = config.GetValue<bool>("Strategies:Weekly_DCA:Enabled", false)
            },
            new() 
            { 
                Name = "LLMAnalysis", 
                IsEnabled = config.GetValue<bool>("Strategies:LLMAnalysis:Enabled", false)
            }
        };
    }

    private void LoadSystemHealth(IServiceScope scope)
    {
        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        
        IsN8NRunning = config.GetValue<bool>("N8N:Enabled", false);
        // TODO: Implement metrics tracking service for actual webhook latency
        LastWebhookLatency = 0; // Placeholder until metrics service is implemented
        AnalysisInterval = config.GetValue<int>("StrategyAnalysis:IntervalMinutes", 15);
        AutoTradingEnabled = config.GetValue<bool>("Trading:EnableAutoTrading", false);
    }
}

// DTOs
public class WatchlistItem
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Change24h { get; set; }
    public string LastSignal { get; set; } = string.Empty;
}

public class SignalItem
{
    public DateTime Timestamp { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Strategy { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
}

public class DecisionItem
{
    public DateTime Timestamp { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
}

public class StrategyStatus
{
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public DateTime? LastRun { get; set; }
}

public static class DashboardHelpers
{
    public static string GetDecisionCssClass(string decision)
    {
        return decision.ToUpper() switch
        {
            "EXECUTE" => "execute",
            "NO_ACTION" => "no_action",
            _ => string.Empty
        };
    }
    
    public static string GetDirectionCssClass(string direction)
    {
        return direction.ToUpper() switch
        {
            "LONG" => "long",
            "SHORT" => "short",
            _ => string.Empty
        };
    }
}
