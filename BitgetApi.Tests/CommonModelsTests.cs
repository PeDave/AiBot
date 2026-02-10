using BitgetApi.Models;
using Xunit;

namespace BitgetApi.Tests;

public class CommonModelsTests
{
    [Fact]
    public void BitgetResponse_IsSuccess_WhenCodeIs00000_ReturnsTrue()
    {
        // Arrange
        var response = new BitgetResponse<string>
        {
            Code = "00000",
            Message = "success",
            Data = "test"
        };

        // Act & Assert
        Assert.True(response.IsSuccess);
    }

    [Fact]
    public void BitgetResponse_IsSuccess_WhenCodeIsNot00000_ReturnsFalse()
    {
        // Arrange
        var response = new BitgetResponse<string>
        {
            Code = "40001",
            Message = "error",
            Data = null
        };

        // Act & Assert
        Assert.False(response.IsSuccess);
    }

    [Fact]
    public void BitgetCredentials_CanBeCreatedWithAllProperties()
    {
        // Arrange & Act
        var credentials = new BitgetCredentials
        {
            ApiKey = "test-api-key",
            SecretKey = "test-secret-key",
            Passphrase = "test-passphrase"
        };

        // Assert
        Assert.Equal("test-api-key", credentials.ApiKey);
        Assert.Equal("test-secret-key", credentials.SecretKey);
        Assert.Equal("test-passphrase", credentials.Passphrase);
    }

    [Fact]
    public void OrderSide_ShouldHaveBuyAndSellValues()
    {
        // Assert
        Assert.Equal(OrderSide.Buy, OrderSide.Buy);
        Assert.Equal(OrderSide.Sell, OrderSide.Sell);
        Assert.NotEqual(OrderSide.Buy, OrderSide.Sell);
    }

    [Fact]
    public void OrderType_ShouldHaveLimitAndMarketValues()
    {
        // Assert
        Assert.Equal(OrderType.Limit, OrderType.Limit);
        Assert.Equal(OrderType.Market, OrderType.Market);
        Assert.NotEqual(OrderType.Limit, OrderType.Market);
    }

    [Fact]
    public void TimeInForce_ShouldHaveAllValues()
    {
        // Assert
        Assert.Equal(TimeInForce.GTC, TimeInForce.GTC);
        Assert.Equal(TimeInForce.IOC, TimeInForce.IOC);
        Assert.Equal(TimeInForce.FOK, TimeInForce.FOK);
        Assert.Equal(TimeInForce.POST_ONLY, TimeInForce.POST_ONLY);
    }
}
