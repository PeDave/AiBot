using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace BitgetApi.TradingEngine.Services;

public class SymbolManager
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SymbolManager> _logger;
    private List<string> _activeSymbols = new();
    private Dictionary<string, string> _symbolReasoning = new();
    private readonly object _lock = new();

    public SymbolManager(IConfiguration configuration, ILogger<SymbolManager> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Initialize with fixed symbols
        var fixedSymbols = _configuration.GetSection("Trading:FixedSymbols").Get<List<string>>() 
                          ?? new List<string> { "BTCUSDT", "ETHUSDT" };
        
        _activeSymbols = new List<string>(fixedSymbols);
        
        _logger.LogInformation("Initialized with fixed symbols: {Symbols}", string.Join(", ", fixedSymbols));
    }

    public void UpdateSymbols(List<string> newSymbols, Dictionary<string, string> reasoning)
    {
        lock (_lock)
        {
            var fixedSymbols = _configuration.GetSection("Trading:FixedSymbols").Get<List<string>>() 
                              ?? new List<string> { "BTCUSDT", "ETHUSDT" };

            // Ensure fixed symbols are always included
            var combinedSymbols = new HashSet<string>(fixedSymbols);
            
            foreach (var symbol in newSymbols)
            {
                combinedSymbols.Add(symbol);
            }

            _activeSymbols = combinedSymbols.ToList();
            _symbolReasoning = reasoning ?? new Dictionary<string, string>();

            _logger.LogInformation("Updated symbols. Total: {Count}, New from N8N: {NewCount}", 
                _activeSymbols.Count, newSymbols.Count);
        }
    }

    public List<string> GetActiveSymbols()
    {
        lock (_lock)
        {
            return new List<string>(_activeSymbols);
        }
    }

    public string? GetSymbolReasoning(string symbol)
    {
        lock (_lock)
        {
            return _symbolReasoning.TryGetValue(symbol, out var reasoning) ? reasoning : null;
        }
    }

    public bool IsSymbolActive(string symbol)
    {
        lock (_lock)
        {
            return _activeSymbols.Contains(symbol);
        }
    }

    public void UpdateWatchlist(List<string> newSymbols)
    {
        string oldSymbolsList;
        string newSymbolsList;
        
        lock (_lock)
        {
            if (newSymbols == null || newSymbols.Count == 0)
            {
                _logger.LogWarning("⚠️ Cannot update watchlist with empty list");
                return;
            }

            oldSymbolsList = string.Join(", ", _activeSymbols);
            _activeSymbols = new List<string>(newSymbols);
            newSymbolsList = string.Join(", ", _activeSymbols);
        }

        _logger.LogInformation("🔄 Watchlist updated:");
        _logger.LogInformation("   Old: {Old}", oldSymbolsList);
        _logger.LogInformation("   New: {New}", newSymbolsList);
    }
}
