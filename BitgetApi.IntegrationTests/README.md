# BitgetApi Integration Tests

This project contains comprehensive integration tests for the Bitget API client library.

## Overview

The integration tests verify that all API endpoints work correctly by making real API calls to Bitget's servers. Tests are organized into two main categories:

- **Public API Tests**: Tests that don't require authentication (market data, server time, etc.)
- **Private API Tests**: Tests that require API credentials (account, trading, etc.)

## Test Structure

```
BitgetApi.IntegrationTests/
├── appsettings.json              # Configuration file (not committed)
├── appsettings.json.example      # Configuration template
├── TestBase.cs                   # Base class for all tests
├── README.md                     # This file
└── PublicApiTests/
    ├── CommonApiTests.cs         # Server time, announcements
    └── SpotMarketTests.cs        # Market data endpoints
```

## Running Tests

### 1. Quick Start (Public API Only)

Public API tests work without any configuration:

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~CommonApiTests"

# Run specific test
dotnet test --filter "Name=GetServerTime_ShouldReturnValidTimestamp"
```

### 2. With API Credentials (Private API)

To run private API tests:

1. **Copy configuration template:**
   ```bash
   cp appsettings.json.example appsettings.json
   ```

2. **Edit `appsettings.json` and add your credentials:**
   ```json
   {
     "Bitget": {
       "ApiKey": "your-api-key",
       "SecretKey": "your-secret-key",
       "Passphrase": "your-passphrase"
     },
     "TestSettings": {
       "TestSymbol": "BTCUSDT",
       "TestFuturesSymbol": "BTCUSDT",
       "TestProductType": "USDT-FUTURES",
       "SkipPrivateTests": false
     }
   }
   ```

3. **Run tests:**
   ```bash
   dotnet test
   ```

## Configuration

### Bitget Credentials

- **ApiKey**: Your Bitget API key
- **SecretKey**: Your Bitget secret key  
- **Passphrase**: Your Bitget API passphrase

**⚠️ Security Warning**: Never commit `appsettings.json` with real credentials!

### Test Settings

- **TestSymbol**: Symbol to use for spot market tests (default: `BTCUSDT`)
- **TestFuturesSymbol**: Symbol to use for futures tests (default: `BTCUSDT`)
- **TestProductType**: Product type for futures (default: `USDT-FUTURES`)
- **SkipPrivateTests**: Set to `false` to enable private API tests (default: `true`)

## Test Categories

### Public API Tests (No Authentication Required)

#### CommonApiTests
- ✅ `GetServerTime_ShouldReturnValidTimestamp` - Verifies server time API
- ✅ `GetAnnouncements_ShouldReturnListOrSkip` - Tests announcements endpoint

#### SpotMarketTests
- ✅ `GetSymbols_ShouldReturnList` - Retrieves all trading pairs
- ✅ `GetTicker_ShouldReturnValidData` - Gets ticker for a specific symbol
- ✅ `GetMarketDepth_ShouldReturnOrderBook` - Fetches order book data
- ✅ `GetRecentTrades_ShouldReturnTrades` - Gets recent trade history
- ✅ `GetCandlesticks_ShouldReturnCandles` - Retrieves OHLCV candlestick data

### Private API Tests (Authentication Required)

_Note: Private API tests will be added in future updates_

## Expected Output

When tests run successfully, you'll see detailed logging:

```
✓ Server time: 1735747200000 (Diff: 123ms)
✓ Announcements: 10 items
✓ Symbols retrieved: 500 symbols
  First symbol: BTCUSDT (BTC/USDT)
✓ Ticker for BTCUSDT:
  Last Price: 42500.00
  24h High: 43000.00
  24h Low: 42000.00
  24h Volume: 1234.5678
✓ Market depth for BTCUSDT:
  Best Bid: 42499.50 (Size: 1.2345)
  Best Ask: 42500.50 (Size: 0.9876)
  Spread: 1.00000000
```

## Troubleshooting

### Configuration File Not Found

**Error**: `System.IO.FileNotFoundException: The configuration file 'appsettings.json' was not found`

**Solution**: Ensure `appsettings.json` exists and has "Copy to Output Directory" set to "Copy if newer"

### Authentication Errors

**Error**: `Authentication not configured or private tests disabled`

**Solution**: 
1. Verify credentials in `appsettings.json`
2. Set `SkipPrivateTests` to `false`
3. Ensure API key has necessary permissions

### API Rate Limiting

If you encounter rate limiting errors:
- Reduce test frequency
- Run tests one at a time using `--filter`
- Wait a few minutes between test runs

## Contributing

When adding new tests:

1. **Follow AAA Pattern**: Arrange, Act, Assert
2. **Use descriptive names**: `MethodName_Scenario_ExpectedResult`
3. **Add detailed logging**: Use `Log()` method for test output
4. **Validate all fields**: Check both structure and data
5. **Handle failures gracefully**: Provide meaningful error messages

### Example Test Structure

```csharp
[Fact]
public async Task GetData_WithValidSymbol_ShouldReturnData()
{
    // Arrange
    var symbol = TestSymbol;

    // Act
    var response = await PublicClient.SomeEndpoint.GetDataAsync(symbol);

    // Assert
    Assert.NotNull(response);
    Assert.True(response.IsSuccess, $"API call failed: {response.Message}");
    Assert.NotNull(response.Data);
    
    Log($"✓ Test passed with {response.Data.Count} items");
}
```

## Additional Resources

- [Bitget API Documentation](https://www.bitget.com/api-doc)
- [xUnit Documentation](https://xunit.net/)
- [Main Repository README](../README.md)

## License

This project is part of the BitgetApi client library. See the main repository for license information.
