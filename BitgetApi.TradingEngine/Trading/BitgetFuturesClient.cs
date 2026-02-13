using BitgetApi;
using BitgetApi.Models;
using BitgetApi.RestApi.Futures;
using Microsoft.Extensions.Logging;

namespace BitgetApi.TradingEngine.Trading;

public class BitgetFuturesClient
{
    private readonly BitgetApiClient _client;
    private readonly ILogger<BitgetFuturesClient> _logger;

    public BitgetFuturesClient(BitgetApiClient client, ILogger<BitgetFuturesClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<string> PlaceMarketOrderAsync(string symbol, string side, decimal size, int leverage)
    {
        try
        {
            // Set leverage first
            await _client.FuturesAccount.SetLeverageAsync(symbol, "USDT", leverage);

            // Place market order
            var response = await _client.FuturesTrade.PlaceOrderAsync(
                symbol: symbol,
                marginCoin: "USDT",
                side: side.ToLower(),
                orderType: "market",
                size: size.ToString("F8")
            );

            if (response?.Code == "00000" && response.Data != null)
            {
                _logger.LogInformation("Futures market order placed: {OrderId} for {Symbol} {Side} {Size}", 
                    response.Data.OrderId, symbol, side, size);
                return response.Data.OrderId;
            }

            _logger.LogError("Failed to place futures market order: {Message}", response?.Message ?? "Unknown error");
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error placing futures market order for {Symbol}", symbol);
            return string.Empty;
        }
    }

    public async Task<string> PlaceLimitOrderAsync(string symbol, string side, decimal size, decimal price, int leverage)
    {
        try
        {
            await _client.FuturesAccount.SetLeverageAsync(symbol, "USDT", leverage);

            var response = await _client.FuturesTrade.PlaceOrderAsync(
                symbol: symbol,
                marginCoin: "USDT",
                side: side.ToLower(),
                orderType: "limit",
                size: size.ToString("F8"),
                price: price.ToString("F2")
            );

            if (response?.Code == "00000" && response.Data != null)
            {
                _logger.LogInformation("Futures limit order placed: {OrderId} for {Symbol} {Side} {Size} @ {Price}", 
                    response.Data.OrderId, symbol, side, size, price);
                return response.Data.OrderId;
            }

            _logger.LogError("Failed to place futures limit order: {Message}", response?.Message ?? "Unknown error");
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error placing futures limit order for {Symbol}", symbol);
            return string.Empty;
        }
    }

    public async Task<bool> SetStopLossAsync(string symbol, decimal stopLossPrice, string side, decimal size)
    {
        try
        {
            var planSide = side.ToLower() == "buy" ? "sell" : "buy"; // Opposite side for SL

            var response = await _client.FuturesTrade.PlacePlanOrderAsync(
                symbol: symbol,
                marginCoin: "USDT",
                side: planSide,
                triggerPrice: stopLossPrice.ToString("F2"),
                executePrice: stopLossPrice.ToString("F2"), 
                size: size.ToString("F8")
            );

            if (response?.Code == "00000")
            {
                _logger.LogInformation("Stop loss set for {Symbol} at {Price}", symbol, stopLossPrice);
                return true;
            }

            _logger.LogError("Failed to set stop loss: {Message}", response?.Message ?? "Unknown error");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting stop loss for {Symbol}", symbol);
            return false;
        }
    }

    public async Task<bool> SetTakeProfitAsync(string symbol, decimal takeProfitPrice, string side, decimal size)
    {
        try
        {
            var planSide = side.ToLower() == "buy" ? "sell" : "buy"; // Opposite side for TP

            var response = await _client.FuturesTrade.PlacePlanOrderAsync(
                symbol: symbol,
                marginCoin: "USDT",
                side: planSide,
                triggerPrice: takeProfitPrice.ToString("F2"),
                executePrice: takeProfitPrice.ToString("F2"),
                size: size.ToString("F8")
            );

            if (response?.Code == "00000")
            {
                _logger.LogInformation("Take profit set for {Symbol} at {Price}", symbol, takeProfitPrice);
                return true;
            }

            _logger.LogError("Failed to set take profit: {Message}", response?.Message ?? "Unknown error");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting take profit for {Symbol}", symbol);
            return false;
        }
    }

    public async Task<bool> ClosePositionAsync(string symbol, string side, decimal size)
    {
        try
        {
            var closeSide = side.ToLower() == "buy" ? "sell" : "buy"; // Opposite side to close

            var response = await _client.FuturesTrade.PlaceOrderAsync(
                symbol: symbol,
                marginCoin: "USDT",
                side: closeSide,
                orderType: "market",
                size: size.ToString("F8")
            );

            if (response?.Code == "00000")
            {
                _logger.LogInformation("Position closed for {Symbol}", symbol);
                return true;
            }

            _logger.LogError("Failed to close position: {Message}", response?.Message ?? "Unknown error");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing position for {Symbol}", symbol);
            return false;
        }
    }

    public async Task<decimal> GetAccountBalanceAsync()
    {
        try
        {
            var response = await _client.FuturesAccount.GetAccountsAsync("USDT-FUTURES");

            if (response?.Code == "00000" && response.Data != null && response.Data.Count > 0)
            {
                var account = response.Data.FirstOrDefault();
                if (account != null && decimal.TryParse(account.Available, out var available))
                {
                    return available;
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting account balance");
            return 0;
        }
    }
}
