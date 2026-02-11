using System.Collections.Concurrent;
using System.Globalization;
using BitgetApi.Dashboard.Models;
using BitgetApi.WebSocket.Public;

namespace BitgetApi.Dashboard.Services;

public class PriceTrackerService
{
    private readonly ConcurrentDictionary<string, PriceUpdate> _prices = new();
    
    public event Action<string, PriceUpdate>? OnPriceUpdated;
    
    public async Task SubscribeAsync(string symbol, BitgetApiClient client, CancellationToken cancellationToken = default)
    {
        await client.SpotPublicChannels.SubscribeTickerAsync(symbol, ticker => 
        {
            var change24h = 0m;
            if (decimal.TryParse(ticker.Open24h, NumberStyles.Any, CultureInfo.InvariantCulture, out var open24h) && open24h > 0)
            {
                if (decimal.TryParse(ticker.LastPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var lastPrice))
                {
                    change24h = ((lastPrice - open24h) / open24h) * 100;
                }
            }
            
            _prices[symbol] = new PriceUpdate 
            {
                Symbol = symbol,
                Price = decimal.Parse(ticker.LastPrice, CultureInfo.InvariantCulture),
                Change24h = change24h,
                Volume = ticker.BaseVolume,
                Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(ticker.Timestamp).UtcDateTime
            };
            OnPriceUpdated?.Invoke(symbol, _prices[symbol]);
        }, cancellationToken);
    }
    
    public PriceUpdate? GetPrice(string symbol) => _prices.TryGetValue(symbol, out var price) ? price : null;
    
    public IEnumerable<string> GetSymbols() => _prices.Keys;
}
