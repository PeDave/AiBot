# Quick Start Guide - Bitget Trading System

## 🚀 Getting Started in 5 Minutes

### Step 1: Open the Solution

**Visual Studio 2022/2026:**
1. Open Visual Studio
2. File → Open → Project/Solution
3. Navigate to `/AiBot/BitgetApi.sln`
4. Click "Open"

**Visual Studio Code:**
```bash
cd AiBot
code .
```

**Command Line:**
```bash
cd AiBot
dotnet build BitgetApi.sln
```

### Step 2: Build the Solution

**Visual Studio:**
- Press `Ctrl+Shift+B` or Build → Build Solution

**Command Line:**
```bash
dotnet build BitgetApi.sln --configuration Release
```

Expected output:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Step 3: Run Tests

**Visual Studio:**
- Test → Run All Tests

**Command Line:**
```bash
dotnet test BitgetApi.Tests/BitgetApi.Tests.csproj
```

Expected output:
```
Passed!  - Failed:     0, Passed:    19, Skipped:     0
```

### Step 4: Run Demo App

**Visual Studio:**
1. Right-click `BitgetApi.Console` project
2. Set as Startup Project
3. Press `F5` or Debug → Start Debugging

**Command Line:**
```bash
cd BitgetApi.Console
dotnet run
```

### Step 5: Try Your First API Call

Create a new C# console app or use the existing demo:

```csharp
using BitgetApi;

// Get BTC price
using var client = new BitgetApiClient();
var ticker = await client.SpotMarket.GetTickerAsync("BTCUSDT");
Console.WriteLine($"BTC/USDT: ${ticker.Data.LastPrice}");
```

## 📝 Next Steps

### For Testing (No API Keys Required)
See public API examples in `BitgetApi.Console/Program.cs`:
- Get server time
- Get market tickers
- Get order book depth
- Get candlestick data

### For Trading (API Keys Required)
1. Get API keys from Bitget Exchange
2. Copy `appsettings.json.example` to `appsettings.json`
3. Add your credentials:
```json
{
  "Bitget": {
    "ApiKey": "your-api-key",
    "SecretKey": "your-secret-key",
    "Passphrase": "your-passphrase"
  }
}
```

4. Use authenticated endpoints:
```csharp
var credentials = new BitgetCredentials { /* from config */ };
using var client = new BitgetApiClient(credentials);
var assets = await client.SpotAccount.GetAccountAssetsAsync();
```

## 🔍 Project Structure

```
BitgetApi/              # Main library - reference this in your projects
BitgetApi.Tests/        # Unit tests - learn from these examples
BitgetApi.Console/      # Demo app - copy code from here
README.md               # Full documentation
IMPLEMENTATION_SUMMARY.md   # Technical details
```

## 💡 Common Use Cases

### 1. Get Market Data
```csharp
using var client = new BitgetApiClient();

// Get ticker
var ticker = await client.SpotMarket.GetTickerAsync("BTCUSDT");

// Get depth
var depth = await client.SpotMarket.GetMarketDepthAsync("BTCUSDT", limit: 10);

// Get candles
var candles = await client.SpotMarket.GetCandlesticksAsync(
    "BTCUSDT", "15min", startTime: 0, endTime: DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
);
```

### 2. Place Orders
```csharp
var credentials = new BitgetCredentials { /* ... */ };
using var client = new BitgetApiClient(credentials);

// Place limit order
var order = await client.SpotTrade.PlaceOrderAsync(
    symbol: "BTCUSDT",
    side: "buy",
    orderType: "limit",
    size: "0.001",
    price: "50000"
);

Console.WriteLine($"Order ID: {order.Data.OrderId}");
```

### 3. WebSocket Streaming
```csharp
using var client = new BitgetApiClient();
await client.WebSocket.ConnectPublicAsync();

await client.SpotPublicChannels.SubscribeTickerAsync("BTCUSDT", ticker =>
{
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] BTC: ${ticker.LastPrice}");
});

await Task.Delay(Timeout.Infinite); // Keep alive
```

### 4. Futures Trading
```csharp
var credentials = new BitgetCredentials { /* ... */ };
using var client = new BitgetApiClient(credentials);

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

## ⚠️ Important Notes

1. **API Keys Security**
   - Never commit API keys to Git
   - Use appsettings.json (already in .gitignore)
   - Or use environment variables

2. **Rate Limits**
   - Library has built-in rate limiting
   - Default: 10 requests/second
   - Automatic retry on rate limit errors

3. **Error Handling**
   - Always check `response.IsSuccess`
   - Wrap in try-catch for network errors
   - See examples in demo app

4. **Testing**
   - Use testnet for development (if available)
   - Start with public endpoints (no keys needed)
   - Test with small amounts first

## 🆘 Troubleshooting

**Build Errors:**
```bash
# Clean and rebuild
dotnet clean
dotnet build
```

**Missing Dependencies:**
```bash
# Restore NuGet packages
dotnet restore
```

**Test Failures:**
```bash
# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"
```

## 📚 Documentation

- **README.md** - Full API reference and documentation
- **IMPLEMENTATION_SUMMARY.md** - Technical implementation details
- **Bitget API Docs** - https://www.bitget.com/api-doc/common/intro

## 🎓 Learning Path

1. ✅ **Start Here** - Run demo app to see it work
2. ✅ **Read README.md** - Understand the API
3. ✅ **Study Tests** - See how components work
4. ✅ **Try Examples** - Modify demo code
5. ✅ **Build Your Bot** - Create your trading strategy

## 🎉 You're Ready!

The Bitget Trading System is now set up and ready to use. Happy trading! 🚀

---

**Need Help?**
- Check README.md for detailed documentation
- Review BitgetApi.Console/Program.cs for working examples
- Consult Bitget API documentation for endpoint details
