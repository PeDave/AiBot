using System.Collections.Concurrent;
using System.Globalization;
using BitgetApi.Dashboard.Models;
using BitgetApi.WebSocket.Public;

namespace BitgetApi.Dashboard.Services;

public class TradeStreamService
{
    private readonly ConcurrentQueue<TradeRecord> _trades = new();
    private readonly int _maxTrades;
    
    public event Action<TradeRecord>? OnTradeReceived;
    
    public TradeStreamService(int maxTrades = 20)
    {
        _maxTrades = maxTrades;
    }
    
    public async Task SubscribeAsync(string symbol, BitgetApiClient client, CancellationToken cancellationToken = default)
    {
        await client.SpotPublicChannels.SubscribeTradesAsync(symbol, tradeData =>
        {
            try
            {
                var trade = new TradeRecord
                {
                    Symbol = symbol,
                    TradeId = tradeData.TradeId,
                    Price = decimal.Parse(tradeData.Price, CultureInfo.InvariantCulture),
                    Size = decimal.Parse(tradeData.Size, CultureInfo.InvariantCulture),
                    Side = tradeData.Side.ToLowerInvariant(),
                    Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(tradeData.Timestamp).UtcDateTime
                };
                
                _trades.Enqueue(trade);
                
                // Keep only the last N trades
                while (_trades.Count > _maxTrades)
                {
                    _trades.TryDequeue(out _);
                }
                
                OnTradeReceived?.Invoke(trade);
            }
            catch (Exception ex)
            {
                // Log parsing error but don't crash - continue processing other messages
                System.Diagnostics.Debug.WriteLine($"Error processing trade for {symbol}: {ex.Message}");
            }
        }, cancellationToken);
    }
    
    public List<TradeRecord> GetRecentTrades()
    {
        // Create a thread-safe copy of the trades and reverse it
        return _trades.ToArray().Reverse().ToList();
    }
    
    public void Clear() => _trades.Clear();
}
