using BitgetApi.TradingEngine.Models;
using Microsoft.Extensions.Logging;

namespace BitgetApi.TradingEngine.Trading;

public class PositionManager
{
    private readonly BitgetFuturesClient _futuresClient;
    private readonly BitgetSpotClient _spotClient;
    private readonly ILogger<PositionManager> _logger;
    private readonly Dictionary<string, Position> _openPositions = new();

    public PositionManager(
        BitgetFuturesClient futuresClient, 
        BitgetSpotClient spotClient,
        ILogger<PositionManager> logger)
    {
        _futuresClient = futuresClient;
        _spotClient = spotClient;
        _logger = logger;
    }

    public async Task<Position?> OpenFuturesPositionAsync(
        Signal signal, 
        decimal positionSizeUsd, 
        int leverage)
    {
        try
        {
            var size = positionSizeUsd / signal.EntryPrice;
            var side = signal.Type == SignalType.LONG ? "buy" : "sell";

            // Place the main order
            var orderId = await _futuresClient.PlaceMarketOrderAsync(signal.Symbol, side, size, leverage);
            
            if (string.IsNullOrEmpty(orderId))
            {
                _logger.LogError("Failed to open futures position for {Symbol}", signal.Symbol);
                return null;
            }

            // Set stop loss and take profit
            await _futuresClient.SetStopLossAsync(signal.Symbol, signal.StopLoss, side, size);
            await _futuresClient.SetTakeProfitAsync(signal.Symbol, signal.TakeProfit, side, size);

            var position = new Position
            {
                Symbol = signal.Symbol,
                Strategy = signal.Strategy,
                Side = signal.Type,
                EntryPrice = signal.EntryPrice,
                Size = size,
                StopLoss = signal.StopLoss,
                TakeProfit = signal.TakeProfit,
                Status = PositionStatus.Open,
                OrderId = orderId,
                Leverage = leverage,
                Market = "Futures",
                OpenTime = DateTime.UtcNow
            };

            _openPositions[GetPositionKey(signal.Symbol, signal.Strategy)] = position;
            _logger.LogInformation("Opened futures position: {Symbol} {Side} {Size} @ {Price} with {Leverage}x leverage", 
                signal.Symbol, signal.Type, size, signal.EntryPrice, leverage);

            return position;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error opening futures position for {Symbol}", signal.Symbol);
            return null;
        }
    }

    public async Task<DcaOrder?> ExecuteSpotDcaAsync(Signal signal, decimal amountUsd)
    {
        try
        {
            var orderId = await _spotClient.PlaceMarketBuyAsync(signal.Symbol, amountUsd);
            
            if (string.IsNullOrEmpty(orderId))
            {
                _logger.LogError("Failed to execute spot DCA for {Symbol}", signal.Symbol);
                return null;
            }

            var price = await _spotClient.GetCurrentPriceAsync(signal.Symbol);
            var quantity = amountUsd / price;

            var dcaType = signal.Metadata.TryGetValue("dca_type", out var type) 
                ? Enum.Parse<DcaOrderType>(type.ToString() ?? "Weekly") 
                : DcaOrderType.Weekly;

            var dcaOrder = new DcaOrder
            {
                Symbol = signal.Symbol,
                AmountUsd = amountUsd,
                Price = price,
                Quantity = quantity,
                Type = dcaType,
                OrderId = orderId,
                Reason = signal.Reason,
                ExecutedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Executed spot DCA: {Symbol} ${Amount} @ {Price} ({Type})", 
                signal.Symbol, amountUsd, price, dcaType);

            return dcaOrder;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing spot DCA for {Symbol}", signal.Symbol);
            return null;
        }
    }

    public async Task<bool> ClosePositionAsync(Position position, decimal exitPrice)
    {
        try
        {
            var side = position.Side == SignalType.LONG ? "buy" : "sell";
            var success = await _futuresClient.ClosePositionAsync(position.Symbol, side, position.Size);

            if (success)
            {
                position.Status = PositionStatus.Closed;
                position.ExitPrice = exitPrice;
                position.CloseTime = DateTime.UtcNow;
                
                // Calculate PnL
                if (position.Side == SignalType.LONG)
                {
                    position.PnL = (exitPrice - position.EntryPrice) * position.Size;
                }
                else
                {
                    position.PnL = (position.EntryPrice - exitPrice) * position.Size;
                }
                
                position.PnLPercent = position.PnL / (position.EntryPrice * position.Size) * 100;

                _openPositions.Remove(GetPositionKey(position.Symbol, position.Strategy));
                
                _logger.LogInformation("Closed position: {Symbol} {Side} PnL: {PnL} ({PnLPercent}%)", 
                    position.Symbol, position.Side, position.PnL, position.PnLPercent);

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing position for {Symbol}", position.Symbol);
            return false;
        }
    }

    public Position? GetOpenPosition(string symbol, string strategy)
    {
        return _openPositions.TryGetValue(GetPositionKey(symbol, strategy), out var position) 
            ? position 
            : null;
    }

    public List<Position> GetAllOpenPositions()
    {
        return _openPositions.Values.ToList();
    }

    public int GetOpenPositionCount()
    {
        return _openPositions.Count;
    }

    public int GetOpenPositionCountForSymbol(string symbol)
    {
        return _openPositions.Values.Count(p => p.Symbol == symbol);
    }

    private string GetPositionKey(string symbol, string strategy)
    {
        return $"{symbol}_{strategy}";
    }
}
