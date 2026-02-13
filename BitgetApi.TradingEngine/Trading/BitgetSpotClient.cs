using BitgetApi;
using BitgetApi.Models;
using BitgetApi.RestApi.Spot;
using Microsoft.Extensions.Logging;

namespace BitgetApi.TradingEngine.Trading;

public class BitgetSpotClient
{
    private readonly BitgetApiClient _client;
    private readonly ILogger<BitgetSpotClient> _logger;

    public BitgetSpotClient(BitgetApiClient client, ILogger<BitgetSpotClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<string> PlaceMarketBuyAsync(string symbol, decimal amountUsd)
    {
        try
        {
            // Get current price to calculate size
            var ticker = await _client.SpotMarket.GetTickerAsync(symbol);
            
            if (ticker?.Code == "00000" && ticker.Data != null && !string.IsNullOrEmpty(ticker.Data.LastPrice))
            {
                var price = decimal.Parse(ticker.Data.LastPrice);
                var size = amountUsd / price;

                var response = await _client.SpotTrade.PlaceOrderAsync(
                    symbol: symbol,
                    side: "buy",
                    orderType: "market",
                    size: size.ToString("F8")
                );

                if (response?.Code == "00000" && response.Data != null)
                {
                    _logger.LogInformation("Spot market buy order placed: {OrderId} for {Symbol} ${Amount}", 
                        response.Data.OrderId, symbol, amountUsd);
                    return response.Data.OrderId;
                }

                _logger.LogError("Failed to place spot market order: {Message}", response?.Msg ?? "Unknown error");
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error placing spot market order for {Symbol}", symbol);
            return string.Empty;
        }
    }

    public async Task<decimal> GetAssetBalanceAsync(string coin)
    {
        try
        {
            var response = await _client.SpotAccount.GetAccountAssetsAsync();

            if (response?.Code == "00000" && response.Data != null)
            {
                var asset = response.Data.FirstOrDefault(a => a.Coin.Equals(coin, StringComparison.OrdinalIgnoreCase));
                
                if (asset != null && decimal.TryParse(asset.Available, out var available))
                {
                    return available;
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting asset balance for {Coin}", coin);
            return 0;
        }
    }

    public async Task<decimal> GetCurrentPriceAsync(string symbol)
    {
        try
        {
            var ticker = await _client.SpotMarket.GetTickerAsync(symbol);
            
            if (ticker?.Code == "00000" && ticker.Data != null && !string.IsNullOrEmpty(ticker.Data.LastPrice))
            {
                return decimal.Parse(ticker.Data.LastPrice);
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current price for {Symbol}", symbol);
            return 0;
        }
    }
}
