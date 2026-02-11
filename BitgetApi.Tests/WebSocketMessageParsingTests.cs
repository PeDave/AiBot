using System.Text.Json;
using BitgetApi.WebSocket;
using BitgetApi.WebSocket.Public;
using Xunit;

namespace BitgetApi.Tests;

public class WebSocketMessageParsingTests
{
    [Fact]
    public void TradeData_ShouldDeserialize_FromActualApiV2Response()
    {
        // Arrange - actual response from Bitget API v2 logs
        var actualTradeResponse = @"{
            ""action"": ""update"",
            ""arg"": {
                ""instType"": ""SPOT"",
                ""channel"": ""trade"",
                ""instId"": ""BTCUSDT""
            },
            ""data"": [{
                ""ts"": ""1770845334290"",
                ""price"": ""68166.87"",
                ""size"": ""0.010131"",
                ""side"": ""sell"",
                ""tradeId"": ""1405592696997527552""
            }]
        }";

        // Act
        var response = JsonSerializer.Deserialize<WebSocketResponse<TradeData>>(actualTradeResponse);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("update", response.Action);
        Assert.NotNull(response.Data);
        Assert.Single(response.Data);

        var trade = response.Data[0];
        Assert.Equal("68166.87", trade.Price);
        Assert.Equal("0.010131", trade.Size);
        Assert.Equal("sell", trade.Side);
        Assert.Equal("1405592696997527552", trade.TradeId);
        Assert.Equal("1770845334290", trade.Timestamp);
    }

    [Fact]
    public void TickerData_ShouldDeserialize_FromActualApiV2Response()
    {
        // Arrange - actual response from Bitget API v2 logs (without ts in data)
        var actualTickerResponse = @"{
            ""action"": ""snapshot"",
            ""arg"": {
                ""instType"": ""SPOT"",
                ""channel"": ""ticker"",
                ""instId"": ""ETHUSDT""
            },
            ""data"": [{
                ""instId"": ""ETHUSDT"",
                ""lastPr"": ""1975.79"",
                ""open24h"": ""1929.45"",
                ""high24h"": ""2032.7"",
                ""low24h"": ""1902.65"",
                ""change24h"": ""-0.01681"",
                ""baseVolume"": ""123456.78"",
                ""quoteVolume"": ""987654.32""
            }]
        }";

        // Act
        var response = JsonSerializer.Deserialize<WebSocketResponse<TickerData>>(actualTickerResponse);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("snapshot", response.Action);
        Assert.NotNull(response.Data);
        Assert.Single(response.Data);

        var ticker = response.Data[0];
        Assert.Equal("ETHUSDT", ticker.Symbol);
        Assert.Equal("1975.79", ticker.LastPrice);
        Assert.Equal("1929.45", ticker.Open24h);
        Assert.Equal("2032.7", ticker.High24h);
        Assert.Equal("1902.65", ticker.Low24h);
        Assert.Equal("123456.78", ticker.BaseVolume);
    }

    [Fact]
    public void DepthData_ShouldDeserialize_FromActualApiV2Response()
    {
        // Arrange - actual response from Bitget API v2 logs (without ts in data)
        var actualDepthResponse = @"{
            ""action"": ""snapshot"",
            ""arg"": {
                ""instType"": ""SPOT"",
                ""channel"": ""books15"",
                ""instId"": ""BTCUSDT""
            },
            ""data"": [{
                ""asks"": [[""68159.61"", ""1.233941""], [""68159.62"", ""0.500000""]],
                ""bids"": [[""68159.6"", ""0.675831""], [""68159.59"", ""1.200000""]]
            }]
        }";

        // Act
        var response = JsonSerializer.Deserialize<WebSocketResponse<DepthData>>(actualDepthResponse);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("snapshot", response.Action);
        Assert.NotNull(response.Data);
        Assert.Single(response.Data);

        var depth = response.Data[0];
        Assert.Equal(2, depth.Asks.Count);
        Assert.Equal(2, depth.Bids.Count);
        Assert.Equal("68159.61", depth.Asks[0][0]);
        Assert.Equal("1.233941", depth.Asks[0][1]);
        Assert.Equal("68159.6", depth.Bids[0][0]);
        Assert.Equal("0.675831", depth.Bids[0][1]);
    }
}
