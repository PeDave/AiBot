using BitgetApi.WebSocket;
using BitgetApi.WebSocket.Public;
using System.Globalization;

namespace BitgetApi.Dashboard.Web.Services;

public class PriceTrackerService
{
    private readonly BitgetWebSocketClient _client;
    private readonly Dictionary<string, PriceData> _prices = new();
    private SpotPublicChannels? _spotChannels;
    
    public event Action? OnPriceUpdated;
    
    public PriceTrackerService(BitgetWebSocketClient client)
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
    
    public async Task SubscribeAsync(string symbol)
    {
        if (_spotChannels == null)
            await InitializeAsync();
        
        await _spotChannels!.SubscribeTickerAsync(symbol, ticker =>
        {
            if (decimal.TryParse(ticker.LastPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var price))
            {
                var change24h = decimal.TryParse(ticker.Open24h, NumberStyles.Any, CultureInfo.InvariantCulture, out var open) && open > 0
                    ? ((price - open) / open) * 100
                    : 0;
                
                _prices[symbol] = new PriceData
                {
                    Symbol = symbol,
                    Price = price,
                    Change24h = change24h,
                    Volume = decimal.TryParse(ticker.BaseVolume, NumberStyles.Any, CultureInfo.InvariantCulture, out var vol) ? vol : 0,
                    High24h = decimal.TryParse(ticker.High24h, NumberStyles.Any, CultureInfo.InvariantCulture, out var high) ? high : 0,
                    Low24h = decimal.TryParse(ticker.Low24h, NumberStyles.Any, CultureInfo.InvariantCulture, out var low) ? low : 0
                };
                
                OnPriceUpdated?.Invoke();
            }
        });
    }
    
    public PriceData? GetPrice(string symbol) 
        => _prices.TryGetValue(symbol, out var price) ? price : null;
    
    public IEnumerable<string> GetSymbols() => _prices.Keys;
}

public record PriceData
{
    public string Symbol { get; init; } = "";
    public decimal Price { get; init; }
    public decimal Change24h { get; init; }
    public decimal Volume { get; init; }
    public decimal High24h { get; init; }
    public decimal Low24h { get; init; }
}
