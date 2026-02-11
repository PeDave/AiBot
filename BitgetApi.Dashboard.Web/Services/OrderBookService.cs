using BitgetApi.WebSocket;
using BitgetApi.WebSocket.Public;

namespace BitgetApi.Dashboard.Web.Services;

public class OrderBookService
{
    private readonly BitgetWebSocketClient _client;
    private readonly Dictionary<string, OrderBookData> _orderBooks = new();
    private SpotPublicChannels? _spotChannels;
    
    public event Action? OnOrderBookUpdated;
    
    public OrderBookService(BitgetWebSocketClient client)
    {
        _client = client;
    }
    
    public async Task InitializeAsync()
    {
        if (_spotChannels == null)
        {
            await _client.ConnectPublicAsync();
            _spotChannels = new SpotPublicChannels(_client);
        }
    }
    
    public async Task SubscribeAsync(string symbol, int depth = 15)
    {
        if (_spotChannels == null)
            await InitializeAsync();
        
        await _spotChannels!.SubscribeDepthAsync(symbol, depth, depthData =>
        {
            var asks = depthData.Asks.Select(a => new OrderLevel
            {
                Price = decimal.Parse(a[0]),
                Size = decimal.Parse(a[1])
            }).ToList();
            
            var bids = depthData.Bids.Select(b => new OrderLevel
            {
                Price = decimal.Parse(b[0]),
                Size = decimal.Parse(b[1])
            }).ToList();
            
            _orderBooks[symbol] = new OrderBookData
            {
                Symbol = symbol,
                Asks = asks,
                Bids = bids,
                LastUpdate = !string.IsNullOrEmpty(depthData.Timestamp) && long.TryParse(depthData.Timestamp, out var ts)
                    ? DateTimeOffset.FromUnixTimeMilliseconds(ts).UtcDateTime
                    : DateTime.UtcNow
            };
            
            OnOrderBookUpdated?.Invoke();
        });
    }
    
    public OrderBookData? GetOrderBook(string symbol)
        => _orderBooks.TryGetValue(symbol, out var book) ? book : null;
}

public record OrderBookData
{
    public string Symbol { get; init; } = "";
    public List<OrderLevel> Asks { get; init; } = new();
    public List<OrderLevel> Bids { get; init; } = new();
    public DateTime LastUpdate { get; init; }
}

public record OrderLevel
{
    public decimal Price { get; init; }
    public decimal Size { get; init; }
}
