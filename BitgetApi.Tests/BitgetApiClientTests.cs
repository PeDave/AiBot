using BitgetApi.Models;
using Xunit;

namespace BitgetApi.Tests;

public class BitgetApiClientTests
{
    [Fact]
    public void Constructor_WithoutCredentials_ShouldCreateClientForPublicEndpoints()
    {
        // Act
        using var client = new BitgetApiClient();

        // Assert
        Assert.NotNull(client);
        Assert.NotNull(client.Common);
        Assert.NotNull(client.SpotMarket);
        Assert.NotNull(client.FuturesMarket);
        Assert.NotNull(client.WebSocket);
    }

    [Fact]
    public void Constructor_WithCredentials_ShouldCreateFullClient()
    {
        // Arrange
        var credentials = new BitgetCredentials
        {
            ApiKey = "test-api-key",
            SecretKey = "test-secret-key",
            Passphrase = "test-passphrase"
        };

        // Act
        using var client = new BitgetApiClient(credentials);

        // Assert
        Assert.NotNull(client);
        Assert.NotNull(client.Common);
        Assert.NotNull(client.SpotMarket);
        Assert.NotNull(client.SpotAccount);
        Assert.NotNull(client.SpotTrade);
        Assert.NotNull(client.FuturesMarket);
        Assert.NotNull(client.FuturesAccount);
        Assert.NotNull(client.FuturesTrade);
        Assert.NotNull(client.MarginAccount);
        Assert.NotNull(client.Earn);
        Assert.NotNull(client.CopyTrading);
        Assert.NotNull(client.Broker);
        Assert.NotNull(client.Convert);
        Assert.NotNull(client.Tax);
        Assert.NotNull(client.WebSocket);
        Assert.NotNull(client.SpotPublicChannels);
        Assert.NotNull(client.FuturesPublicChannels);
        Assert.NotNull(client.SpotPrivateChannels);
        Assert.NotNull(client.FuturesPrivateChannels);
    }

    [Fact]
    public void Create_WithParameters_ShouldCreateClient()
    {
        // Act
        using var client = BitgetApiClient.Create("api-key", "secret-key", "passphrase");

        // Assert
        Assert.NotNull(client);
        Assert.NotNull(client.SpotMarket);
        Assert.NotNull(client.FuturesMarket);
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var client = new BitgetApiClient();

        // Act & Assert
        client.Dispose();
    }
}
