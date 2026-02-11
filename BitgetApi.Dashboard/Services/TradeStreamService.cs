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
        }, cancellationToken);
    }
    
    public List<TradeRecord> GetRecentTrades() => _trades.Reverse().ToList();
    
    public void Clear() => _trades.Clear();
}
