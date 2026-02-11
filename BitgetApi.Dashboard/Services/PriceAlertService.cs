using System.Collections.Concurrent;
using BitgetApi.Dashboard.Models;
using Spectre.Console;

namespace BitgetApi.Dashboard.Services;

public class PriceAlertService
{
    private readonly ConcurrentDictionary<string, decimal> _alerts = new();
    private readonly ConcurrentDictionary<string, bool> _triggered = new();
    
    public void SetAlert(string symbol, decimal targetPrice)
    {
        _alerts[symbol] = targetPrice;
        _triggered[symbol] = false;
    }
    
    public void RemoveAlert(string symbol)
    {
        _alerts.TryRemove(symbol, out _);
        _triggered.TryRemove(symbol, out _);
    }
    
    public void CheckAlerts(PriceUpdate price)
    {
        if (_alerts.TryGetValue(price.Symbol, out var target))
        {
            // Check if not already triggered
            if (_triggered.TryGetValue(price.Symbol, out var triggered) && !triggered)
            {
                if (price.Price >= target)
                {
                    AnsiConsole.MarkupLine($"[yellow]🔔 ALERT: {price.Symbol} reached ${target:N2}! Current price: ${price.Price:N2}[/]");
                    _triggered[price.Symbol] = true;
                }
            }
        }
    }
    
    public Dictionary<string, decimal> GetActiveAlerts()
    {
        return _alerts.Where(kvp => _triggered.TryGetValue(kvp.Key, out var t) && !t)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
