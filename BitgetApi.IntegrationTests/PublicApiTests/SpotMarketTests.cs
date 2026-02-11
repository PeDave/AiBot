using Xunit;
using Xunit.Abstractions;

namespace BitgetApi.IntegrationTests.PublicApiTests;

public class SpotMarketTests : TestBase
{
    public SpotMarketTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task GetSymbols_ShouldReturnList()
    {
        // Act
        var response = await PublicClient.SpotMarket.GetSymbolsAsync();

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsSuccess, $"API call failed: {response.Message}");
        Assert.NotNull(response.Data);
        Assert.NotEmpty(response.Data);
        
        var firstSymbol = response.Data[0];
        Assert.NotNull(firstSymbol.Symbol);
        Assert.NotNull(firstSymbol.BaseCoin);
        Assert.NotNull(firstSymbol.QuoteCoin);
        
        Log($"✓ Symbols retrieved: {response.Data.Count} symbols");
        Log($"  First symbol: {firstSymbol.Symbol} ({firstSymbol.BaseCoin}/{firstSymbol.QuoteCoin})");
    }

    [Fact]
    public async Task GetTicker_ShouldReturnValidData()
    {
        // Act
        var response = await PublicClient.SpotMarket.GetTickerAsync(TestSymbol);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsSuccess, $"API call failed: {response.Message}");
        Assert.NotNull(response.Data);
        Assert.Equal(TestSymbol, response.Data.Symbol);
        Assert.NotEmpty(response.Data.LastPrice);
        Assert.NotEmpty(response.Data.High24h);
        Assert.NotEmpty(response.Data.Low24h);
        Assert.NotEmpty(response.Data.BaseVolume);
        Assert.NotEmpty(response.Data.QuoteVolume);
        
        Log($"✓ Ticker for {TestSymbol}:");
        Log($"  Last Price: {response.Data.LastPrice}");
        Log($"  24h High: {response.Data.High24h}");
        Log($"  24h Low: {response.Data.Low24h}");
        Log($"  24h Volume: {response.Data.BaseVolume}");
    }

    [Fact]
    public async Task GetMarketDepth_ShouldReturnOrderBook()
    {
        // Act
        var response = await PublicClient.SpotMarket.GetMarketDepthAsync(TestSymbol, limit: 10);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsSuccess, $"API call failed: {response.Message}");
        Assert.NotNull(response.Data);
        Assert.NotNull(response.Data.Bids);
        Assert.NotNull(response.Data.Asks);
        Assert.NotEmpty(response.Data.Bids);
        Assert.NotEmpty(response.Data.Asks);

        var bestBid = response.Data.Bids[0];
        var bestAsk = response.Data.Asks[0];
        Assert.True(bestBid.Count >= 2, "Bid should have at least price and size");
        Assert.True(bestAsk.Count >= 2, "Ask should have at least price and size");
        Assert.NotEmpty(bestBid[0]); // Price
        Assert.NotEmpty(bestBid[1]); // Size
        Assert.NotEmpty(bestAsk[0]); // Price
        Assert.NotEmpty(bestAsk[1]); // Size

        Log($"✓ Market depth for {TestSymbol}:");
        Log($"  Best Bid: {bestBid[0]} (Size: {bestBid[1]})");
        Log($"  Best Ask: {bestAsk[0]} (Size: {bestAsk[1]})");

        // Calculate spread with InvariantCulture
        var bidPrice = decimal.Parse(bestBid[0],
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture);
        var askPrice = decimal.Parse(bestAsk[0],
            System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture);
        var spread = askPrice - bidPrice;

        Log($"  Spread: ${spread:F8}");
    }

    [Fact]
    public async Task GetRecentTrades_ShouldReturnTrades()
    {
        // Act
        var response = await PublicClient.SpotMarket.GetRecentTradesAsync(TestSymbol, limit: 10);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsSuccess, $"API call failed: {response.Message}");
        Assert.NotNull(response.Data);
        Assert.NotEmpty(response.Data);
        
        var firstTrade = response.Data[0];
        Assert.NotEmpty(firstTrade.TradeId);
        Assert.NotEmpty(firstTrade.Price);
        Assert.NotEmpty(firstTrade.Size);
        Assert.NotEmpty(firstTrade.Side);
        Assert.NotEmpty(firstTrade.Timestamp);
        
        Log($"✓ Recent trades for {TestSymbol}: {response.Data.Count} trades");
        Log($"  Latest trade:");
        Log($"    ID: {firstTrade.TradeId}");
        Log($"    Price: {firstTrade.Price}");
        Log($"    Size: {firstTrade.Size}");
        Log($"    Side: {firstTrade.Side}");
    }

    [Fact]
    public async Task GetCandlesticks_ShouldReturnCandles()
    {
        // Arrange
        var granularity = "1min";
        var endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var startTime = endTime - (60 * 1000 * 10); // 10 minutes ago

        // Act
        var response = await PublicClient.SpotMarket.GetCandlesticksAsync(
            TestSymbol, 
            granularity, 
            startTime, 
            endTime);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.IsSuccess, $"API call failed: {response.Message}");
        Assert.NotNull(response.Data);
        Assert.NotEmpty(response.Data);
        
        var firstCandle = response.Data[0];
        Assert.True(firstCandle.Timestamp > 0);
        Assert.NotEmpty(firstCandle.Open);
        Assert.NotEmpty(firstCandle.High);
        Assert.NotEmpty(firstCandle.Low);
        Assert.NotEmpty(firstCandle.Close);
        Assert.NotEmpty(firstCandle.BaseVolume);
        Assert.NotEmpty(firstCandle.QuoteVolume);
        
        Log($"✓ Candlesticks for {TestSymbol} ({granularity}): {response.Data.Count} candles");
        Log($"  First candle:");
        Log($"    Timestamp: {firstCandle.Timestamp}");
        Log($"    OHLC: {firstCandle.Open} / {firstCandle.High} / {firstCandle.Low} / {firstCandle.Close}");
        Log($"    Volume: {firstCandle.BaseVolume}");
    }
}
