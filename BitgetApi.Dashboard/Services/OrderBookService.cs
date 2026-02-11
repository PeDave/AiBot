using System.Globalization;
using BitgetApi.Dashboard.Models;
using BitgetApi.WebSocket.Public;

namespace BitgetApi.Dashboard.Services;

public class OrderBookService
{
    private OrderBookSnapshot? _snapshot;
    private readonly object _snapshotLock = new();
    
    public event Action<OrderBookSnapshot>? OnOrderBookUpdated;
    
    public async Task SubscribeAsync(string symbol, BitgetApiClient client, int depth = 5, CancellationToken cancellationToken = default)
    {
        await client.SpotPublicChannels.SubscribeDepthAsync(symbol, depth, depthData =>
        {
            try
            {
                var newSnapshot = new OrderBookSnapshot
                {
                    Symbol = symbol,
                    Bids = depthData.Bids.Take(depth).Select(ParseLevel).ToList(),
                    Asks = depthData.Asks.Take(depth).Select(ParseLevel).ToList(),
                    Timestamp = !string.IsNullOrEmpty(depthData.Timestamp) && long.TryParse(depthData.Timestamp, out var ts)
                        ? DateTimeOffset.FromUnixTimeMilliseconds(ts).UtcDateTime
                        : DateTime.UtcNow
                };
                
                lock (_snapshotLock)
                {
                    _snapshot = newSnapshot;
                }
                
                var handler = OnOrderBookUpdated;
                handler?.Invoke(newSnapshot);
            }
            catch (Exception ex)
            {
                // Log parsing error but don't crash - continue processing other messages
                System.Diagnostics.Debug.WriteLine($"Error processing order book for {symbol}: {ex.Message}");
            }
        }, cancellationToken);
    }
    
    private (decimal Price, decimal Size) ParseLevel(List<string> level)
    {
        return (
            decimal.Parse(level[0], CultureInfo.InvariantCulture),
            decimal.Parse(level[1], CultureInfo.InvariantCulture)
        );
    }
    
    public OrderBookSnapshot? GetSnapshot()
    {
        lock (_snapshotLock)
        {
            return _snapshot;
        }
    }
}
