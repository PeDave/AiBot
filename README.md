# Bitget API - Complete C# Client Library

A comprehensive, production-ready C# library for the [Bitget Exchange API](https://www.bitget.com/api-doc/common/intro). This library provides full coverage of Bitget's REST API and WebSocket streams for spot trading, futures trading, margin trading, and more.

## Features

✅ **Complete API Coverage**
- All REST API endpoints (Common, Spot, Futures, Margin, Earn, Copy Trading, Broker, Convert, Tax)
- WebSocket support for real-time data (public and private channels)
- HMAC-SHA256 authentication
- Automatic rate limiting and retry logic

✅ **Production Ready**
- .NET 8.0 target framework
- Async/await pattern throughout
- Comprehensive error handling
- XML documentation for IntelliSense
- Unit tests with xUnit and Moq

✅ **Easy to Use**
- Simple, intuitive API
- Strongly typed models
- Automatic request signing
- Built-in reconnection for WebSocket

## Installation

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022/2026 or VS Code

### Building from Source

```bash
git clone https://github.com/PeDave/AiBot.git
cd AiBot
dotnet build BitgetApi.sln
```

### Running Tests

```bash
dotnet test BitgetApi.Tests/BitgetApi.Tests.csproj
```

## Quick Start

### 1. Basic Usage (Public Endpoints)

```csharp
using BitgetApi;

// Create client without credentials for public endpoints
using var client = new BitgetApiClient();

// Get server time
var serverTime = await client.Common.GetServerTimeAsync();
Console.WriteLine($"Server time: {serverTime.Data.ServerTime}");

// Get BTC/USDT ticker
var ticker = await client.SpotMarket.GetTickerAsync("BTCUSDT");
Console.WriteLine($"BTC Price: {ticker.Data.LastPrice}");

// Get market depth
var depth = await client.SpotMarket.GetMarketDepthAsync("BTCUSDT", limit: 10);
Console.WriteLine($"Best bid: {depth.Data.Bids[0].Price}");
Console.WriteLine($"Best ask: {depth.Data.Asks[0].Price}");
```

### 2. Authenticated Endpoints

```csharp
using BitgetApi;
using BitgetApi.Models;

// Create credentials
var credentials = new BitgetCredentials
{
    ApiKey = "your-api-key",
    SecretKey = "your-secret-key",
    Passphrase = "your-passphrase"
};

// Create client with credentials
using var client = new BitgetApiClient(credentials);

// Get account assets
var assets = await client.SpotAccount.GetAccountAssetsAsync();
foreach (var asset in assets.Data)
{
    if (decimal.Parse(asset.Available) > 0)
    {
        Console.WriteLine($"{asset.Coin}: {asset.Available}");
    }
}

// Place a limit order
var order = await client.SpotTrade.PlaceOrderAsync(
    symbol: "BTCUSDT",
    side: "buy",
    orderType: "limit",
    size: "0.001",
    price: "50000"
);

Console.WriteLine($"Order placed: {order.Data.OrderId}");
```

### 3. WebSocket Real-time Data

```csharp
using BitgetApi;

using var client = new BitgetApiClient();

// Connect to public WebSocket
await client.WebSocket.ConnectPublicAsync();

// Subscribe to ticker updates
await client.SpotPublicChannels.SubscribeTickerAsync("BTCUSDT", ticker =>
{
    Console.WriteLine($"Price: {ticker.LastPrice}, Volume: {ticker.BaseVolume}");
});

// Subscribe to trades
await client.SpotPublicChannels.SubscribeTradesAsync("BTCUSDT", trade =>
{
    Console.WriteLine($"Trade: {trade.Side} {trade.Size} @ {trade.Price}");
});

// Keep the connection alive
await Task.Delay(Timeout.Infinite);
```

### 4. Futures Trading

```csharp
using BitgetApi;
using BitgetApi.Models;

var credentials = new BitgetCredentials { /* ... */ };
using var client = new BitgetApiClient(credentials);

// Get futures contracts
var contracts = await client.FuturesMarket.GetContractsAsync("USDT-FUTURES");

// Get account info
var account = await client.FuturesAccount.GetAccountAsync(
    symbol: "BTCUSDT",
    marginCoin: "USDT"
);

// Set leverage
await client.FuturesAccount.SetLeverageAsync(
    symbol: "BTCUSDT",
    marginCoin: "USDT",
    leverage: 10
);

// Place futures order
var futuresOrder = await client.FuturesTrade.PlaceOrderAsync(
    symbol: "BTCUSDT",
    marginCoin: "USDT",
    side: "buy",
    orderType: "limit",
    size: "0.01",
    price: "50000"
);
```

## Configuration

### Using appsettings.json

```json
{
  "Bitget": {
    "ApiKey": "your-api-key",
    "SecretKey": "your-secret-key",
    "Passphrase": "your-passphrase"
  }
}
```

## Project Structure

```
BitgetApi/                      # Main library
├── Auth/                       # Authentication
├── Http/                       # HTTP client
├── Models/                     # Data models
├── RestApi/                    # REST API clients
│   ├── Common/
│   ├── Spot/
│   ├── Futures/
│   ├── Margin/
│   ├── Earn/
│   ├── CopyTrading/
│   ├── Broker/
│   ├── Convert/
│   └── Tax/
└── WebSocket/                  # WebSocket clients
    ├── Public/
    └── Private/

BitgetApi.Tests/                # Unit tests
BitgetApi.Console/              # Demo application
```

## Support

- **Documentation**: [Bitget API Docs](https://www.bitget.com/api-doc/common/intro)
- **Issues**: [GitHub Issues](https://github.com/PeDave/AiBot/issues)

## License

This project is licensed under the MIT License.

## Disclaimer

This library is not officially affiliated with Bitget. Use at your own risk. Always test thoroughly before using in production.

Trading cryptocurrencies carries risk. Never trade with money you cannot afford to lose.
