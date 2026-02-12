using BitgetApi.WebSocket;
using BitgetApi.WebSocket.Public;
using Microsoft.Extensions.Logging;

namespace BitgetApi.Dashboard.Web.Services;

public class OrderBookService
{
    private readonly BitgetWebSocketClient _client;
    private readonly ILogger<OrderBookService>? _logger;
    private readonly Dictionary<string, OrderBookData> _orderBooks = new();
    private SpotPublicChannels? _spotChannels;
    
    public event Action? OnOrderBookUpdated;
    
    public OrderBookService(BitgetWebSocketClient client, ILogger<OrderBookService>? logger = null)
    {
        _client = client;
        _logger = logger;
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
            try
            {
                var asks = depthData.Asks.Select(a =>
                {
                    decimal.TryParse(a[0], out var p);
                    decimal.TryParse(a[1], out var s);
                    return new OrderLevel { Price = p, Size = s };
                }).ToList();

                var bids = depthData.Bids.Select(b =>
                {
                    decimal.TryParse(b[0], out var p);
                    decimal.TryParse(b[1], out var s);
                    return new OrderLevel { Price = p, Size = s };
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
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[OrderBook ERROR] {ex.Message}");
            }
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
