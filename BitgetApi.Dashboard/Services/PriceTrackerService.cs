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
            try
            {
                var change24h = 0m;
                if (decimal.TryParse(ticker.Open24h, NumberStyles.Any, CultureInfo.InvariantCulture, out var open24h) && open24h > 0)
                {
                    if (decimal.TryParse(ticker.LastPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var currentPrice))
                    {
                        change24h = ((currentPrice - open24h) / open24h) * 100;
                    }
                }

                var priceUpdate = new PriceUpdate
                {
                    Symbol = symbol,
                    LastPrice = decimal.TryParse(ticker.LastPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out var lastPrice) ? lastPrice : 0,
                    High24h = decimal.TryParse(ticker.High24h, NumberStyles.Any, CultureInfo.InvariantCulture, out var high) ? high : 0,
                    Low24h = decimal.TryParse(ticker.Low24h, NumberStyles.Any, CultureInfo.InvariantCulture, out var low) ? low : 0,
                    Volume24h = decimal.TryParse(ticker.BaseVolume, NumberStyles.Any, CultureInfo.InvariantCulture, out var vol) ? vol : 0,
                    Change24h = change24h,
                    Timestamp = ticker.Timestamp > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(ticker.Timestamp).UtcDateTime : DateTime.UtcNow
                };

                _prices[symbol] = priceUpdate;

                var handler = OnPriceUpdated;
                handler?.Invoke(symbol, priceUpdate);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing ticker for {symbol}: {ex.Message}");
            }
        }, cancellationToken);
    }

    public PriceUpdate? GetPrice(string symbol) => _prices.TryGetValue(symbol, out var price) ? price : null;

    public IEnumerable<string> GetSymbols() => _prices.Keys;
}