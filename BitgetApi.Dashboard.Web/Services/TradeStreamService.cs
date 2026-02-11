using BitgetApi.WebSocket;
using BitgetApi.WebSocket.Public;
using Microsoft.Extensions.Logging;

namespace BitgetApi.Dashboard.Web.Services;

public class TradeStreamService
{
    private readonly BitgetWebSocketClient _client;
    private readonly ILogger<TradeStreamService>? _logger;
    private readonly Dictionary<string, Queue<TradeData>> _trades = new();
    private SpotPublicChannels? _spotChannels;
    private const int MaxTradesPerSymbol = 50;
    
    public event Action? OnTradeReceived;
    
    public TradeStreamService(BitgetWebSocketClient client, ILogger<TradeStreamService>? logger = null)
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
    
    public async Task SubscribeAsync(string symbol)
    {
        if (_spotChannels == null)
            await InitializeAsync();
        
        if (!_trades.ContainsKey(symbol))
            _trades[symbol] = new Queue<TradeData>();
        
        await _spotChannels!.SubscribeTradesAsync(symbol, trade =>
        {
            DateTime timestamp;
            if (!string.IsNullOrEmpty(trade.Timestamp) && long.TryParse(trade.Timestamp, out var ts))
            {
                timestamp = DateTimeOffset.FromUnixTimeMilliseconds(ts).UtcDateTime;
            }
            else
            {
                _logger?.LogWarning("Failed to parse trade timestamp for {Symbol}, using current time", symbol);
                timestamp = DateTime.UtcNow;
            }
            
            var tradeData = new TradeData
            {
                Symbol = symbol,
                Price = decimal.Parse(trade.Price),
                Size = decimal.Parse(trade.Size),
                Side = trade.Side,
                Timestamp = timestamp
            };
            
            var queue = _trades[symbol];
            queue.Enqueue(tradeData);
            
            while (queue.Count > MaxTradesPerSymbol)
                queue.Dequeue();
            
            OnTradeReceived?.Invoke();
        });
    }
    
    public IEnumerable<TradeData> GetRecentTrades(string symbol)
        => _trades.TryGetValue(symbol, out var trades) ? trades.Reverse() : Enumerable.Empty<TradeData>();
}

public record TradeData
{
    public string Symbol { get; init; } = "";
    public decimal Price { get; init; }
    public decimal Size { get; init; }
    public string Side { get; init; } = "";
    public DateTime Timestamp { get; init; }
}
