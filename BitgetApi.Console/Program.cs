using BitgetApi;
using BitgetApi.Models;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace BitgetApi.Console;

class Program
{
    static async Task Main(string[] args)
    {
        System.Console.WriteLine("=================================================");
        System.Console.WriteLine("    Bitget Trading System - Demo Application");
        System.Console.WriteLine("=================================================");
        System.Console.WriteLine();

        // Demo 1: Public API - No authentication required
        //await DemoPublicApiAsync();

        System.Console.WriteLine();
        System.Console.WriteLine("Press any key to continue to authenticated examples (requires API keys)...");
        System.Console.ReadKey();
        System.Console.WriteLine();

        // Demo 2: Private API - Authentication required
        // Uncomment and add your credentials to test authenticated endpoints
        //await DemoPrivateApiAsync();

        // Demo 3: WebSocket - Real-time data
        //await DemoWebSocketAsync();

        // Demo 4: Advanced Public API
        //await DemoAdvancedPublicApiAsync();

        System.Console.WriteLine();
        System.Console.WriteLine("Demo completed. Press any key to exit...");
        System.Console.ReadKey();
    }

    static async Task DemoPublicApiAsync()
    {
        System.Console.WriteLine("--- Demo 1: Public API Endpoints ---");
        System.Console.WriteLine();

        // Create client without credentials for public endpoints
        using var client = new BitgetApiClient();

        try
        {
            // Get server time
            System.Console.WriteLine("1. Getting server time...");
            var serverTimeResponse = await client.Common.GetServerTimeAsync();
            if (serverTimeResponse.IsSuccess && serverTimeResponse.Data != null)
            {
                System.Console.WriteLine($"   Server Time: {serverTimeResponse.Data.ServerTime}");
                var dateTime = DateTimeOffset.FromUnixTimeMilliseconds(serverTimeResponse.Data.ServerTimeMilliseconds);
                System.Console.WriteLine($"   Date/Time: {dateTime:yyyy-MM-dd HH:mm:ss} UTC");
            }
            System.Console.WriteLine();

            // Get spot symbols
            System.Console.WriteLine("2. Getting spot symbols (first 5)...");
            var symbolsResponse = await client.SpotMarket.GetSymbolsAsync();
            if (symbolsResponse.IsSuccess && symbolsResponse.Data != null)
            {
                System.Console.WriteLine($"   Total Symbols: {symbolsResponse.Data.Count}");
                foreach (var symbol in symbolsResponse.Data.Take(5))
                {
                    System.Console.WriteLine($"   - {symbol.Symbol}: {symbol.BaseCoin}/{symbol.QuoteCoin} (Status: {symbol.Status})");
                }
            }
            System.Console.WriteLine();

            // Get BTC/USDT ticker
            System.Console.WriteLine("3. Getting BTCUSDT ticker...");
            var tickerResponse = await client.SpotMarket.GetTickerAsync("BTCUSDT");
            if (tickerResponse.IsSuccess && tickerResponse.Data != null)
            {
                var ticker = tickerResponse.Data;
                System.Console.WriteLine($"   Symbol: {ticker.Symbol}");
                System.Console.WriteLine($"   Last Price: {ticker.LastPrice}");
                System.Console.WriteLine($"   24h High: {ticker.High24h}");
                System.Console.WriteLine($"   24h Low: {ticker.Low24h}");
                System.Console.WriteLine($"   24h Volume: {ticker.BaseVolume}");
                System.Console.WriteLine($"   24h Change: {ticker.Change24h}%");
            }
            System.Console.WriteLine();

            // Get market depth
            System.Console.WriteLine("4. Getting BTCUSDT order book (top 5)...");
            var depthResponse = await client.SpotMarket.GetMarketDepthAsync("BTCUSDT", limit: 5);
            if (depthResponse.IsSuccess && depthResponse.Data != null)
            {
                System.Console.WriteLine($"   Top 5 Bids:");
                foreach (var bid in depthResponse.Data.Bids.Take(5))
                {
                    var price = bid[0];  // Első elem = ár
                    var size = bid[1];   // Második elem = mennyiség
                    System.Console.WriteLine($"     ${price} - {size}");
                }

                System.Console.WriteLine($"   Top 5 Asks:");
                foreach (var ask in depthResponse.Data.Asks.Take(5))
                {
                    var price = ask[0];
                    var size = ask[1];
                    System.Console.WriteLine($"     ${price} - {size}");
                }
            }
            System.Console.WriteLine();

            // Get futures contracts
            System.Console.WriteLine("5. Getting USDT futures contracts (first 3)...");
            var contractsResponse = await client.FuturesMarket.GetContractsAsync("USDT-FUTURES");
            if (contractsResponse.IsSuccess && contractsResponse.Data != null)
            {
                System.Console.WriteLine($"   Total Contracts: {contractsResponse.Data.Count}");
                foreach (var contract in contractsResponse.Data.Take(3))
                {
                    System.Console.WriteLine($"   - {contract.Symbol}: {contract.BaseCoin}/{contract.QuoteCoin}");
                    System.Console.WriteLine($"     Min Trade: {contract.MinTradeNum}, Maker Fee: {contract.MakerFeeRate}");
                }
            }
            System.Console.WriteLine();

            System.Console.WriteLine("✓ Public API demo completed successfully!");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    static async Task DemoPrivateApiAsync()
    {
        System.Console.WriteLine("--- Demo 2: Private API Endpoints (Authentication Required) ---");
        System.Console.WriteLine();

        // ✅ Load credentials from appsettings.json
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var credentials = new BitgetCredentials
        {
            ApiKey = config["Bitget:ApiKey"] ?? string.Empty,
            SecretKey = config["Bitget:SecretKey"] ?? string.Empty,
            Passphrase = config["Bitget:Passphrase"] ?? string.Empty
        };

        // Validate credentials
        if (string.IsNullOrEmpty(credentials.ApiKey) ||
            string.IsNullOrEmpty(credentials.SecretKey) ||
            string.IsNullOrEmpty(credentials.Passphrase))
        {
            System.Console.WriteLine("⚠️  API credentials not configured!");
            System.Console.WriteLine("   Please edit appsettings.json and add your Bitget API keys.");
            System.Console.WriteLine();
            return;
        }

        System.Console.WriteLine($"✓ Loaded credentials for API Key: {credentials.ApiKey.Substring(0, Math.Min(10, credentials.ApiKey.Length))}...");
        System.Console.WriteLine();

        using var client = new BitgetApiClient(credentials);

        try
        {
            // Get spot account assets
            System.Console.WriteLine("1. Getting spot account assets...");
            var assetsResponse = await client.SpotAccount.GetAccountAssetsAsync();
            if (assetsResponse.IsSuccess && assetsResponse.Data != null)
            {
                var nonZeroAssets = assetsResponse.Data
                    .Where(a => decimal.TryParse(a.Available, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount) && amount > 0)
                    .ToList();
                if (nonZeroAssets.Any())
                {
                    foreach (var asset in nonZeroAssets.Take(10))
                    {
                        System.Console.WriteLine($"   {asset.Coin}: Available={asset.Available}, Frozen={asset.Frozen}, USDT Value={asset.UsdtValue}");
                    }
                }
                else
                {
                    System.Console.WriteLine("   No assets found (account might be empty)");
                }
            }
            else
            {
                System.Console.WriteLine($"   Error: {assetsResponse.Message}");
            }
            System.Console.WriteLine();

            // Get open orders
            System.Console.WriteLine("2. Getting open spot orders...");
            var ordersResponse = await client.SpotTrade.GetOpenOrdersAsync();
            if (ordersResponse.IsSuccess && ordersResponse.Data != null)
            {
                System.Console.WriteLine($"   Total Open Orders: {ordersResponse.Data.Count}");
                if (ordersResponse.Data.Any())
                {
                    foreach (var order in ordersResponse.Data.Take(5))
                    {
                        System.Console.WriteLine($"   - {order.Symbol}: {order.Side} {order.Size} @ {order.Price} (Status: {order.Status})");
                    }
                }
                else
                {
                    System.Console.WriteLine("   No open orders");
                }
            }
            else
            {
                System.Console.WriteLine($"   Error: {assetsResponse.Message}");
            }
            System.Console.WriteLine();

            System.Console.WriteLine("✓ Private API demo completed successfully!");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"✗ Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                System.Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            }
        }
    }

    static async Task DemoWebSocketAsync()
    {
        System.Console.WriteLine("--- Demo 3: WebSocket Real-time Data ---");
        System.Console.WriteLine();

        using var client = new BitgetApiClient();

        try
        {
            System.Console.WriteLine("Connecting to WebSocket...");
            await client.WebSocket.ConnectPublicAsync();

            var messageCount = 0;
            var maxMessages = 10;

            System.Console.WriteLine("Subscribing to BTCUSDT ticker updates...");
            await client.SpotPublicChannels.SubscribeTickerAsync("BTCUSDT", ticker =>
            {
                if (messageCount++ < maxMessages)
                {
                    System.Console.WriteLine($"   [{DateTime.Now:HH:mm:ss}] Price: {ticker.LastPrice}, Volume: {ticker.BaseVolume}");
                }
            });

            System.Console.WriteLine($"Listening for {maxMessages} ticker updates...");
            System.Console.WriteLine();

            // Wait for messages
            await Task.Delay(TimeSpan.FromSeconds(30));

            System.Console.WriteLine();
            System.Console.WriteLine("✓ WebSocket demo completed!");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"✗ Error: {ex.Message}");
        }
    }

    static async Task DemoAdvancedPublicApiAsync()
    {
        System.Console.WriteLine("--- Demo 4: Advanced Public API ---");
        System.Console.WriteLine();

        using var client = new BitgetApiClient();

        try
        {
            // 1. Recent Trades
            System.Console.WriteLine("1. Getting recent BTCUSDT trades...");
            var tradesResponse = await client.SpotMarket.GetRecentTradesAsync("BTCUSDT", limit: 10);
            if (tradesResponse.IsSuccess && tradesResponse.Data != null)
            {
                System.Console.WriteLine($"   Total Trades: {tradesResponse.Data.Count}");
                foreach (var trade in tradesResponse.Data.Take(5))
                {
                    var time = trade.Timestamp;
                    System.Console.WriteLine($"   {time:HH:mm:ss} - {trade.Side.ToUpper(),-4} {trade.Size} BTC @ ${trade.Price}");
                }
            }
            else
            {
                System.Console.WriteLine($"   ✗ Error: {tradesResponse.Message}");
            }
            System.Console.WriteLine();

            // 2. Candlesticks
            System.Console.WriteLine("2. Getting BTCUSDT candlesticks (15min, last 2 hours)...");
            var endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var startTime = endTime - (2 * 60 * 60 * 1000);

            var candlesResponse = await client.SpotMarket.GetCandlesticksAsync(
                "BTCUSDT", "15min", startTime, endTime
            );

            if (candlesResponse.IsSuccess && candlesResponse.Data != null)
            {
                System.Console.WriteLine($"   Total Candles: {candlesResponse.Data.Count}");

                foreach (var candle in candlesResponse.Data.Take(5))
                {
                    // Debug: print raw timestamp
                    System.Console.WriteLine($"   DEBUG - Raw TS: {candle.Timestamp}");

                    if (candle.Timestamp > 0)
                    {
                        var time = DateTimeOffset.FromUnixTimeMilliseconds(candle.Timestamp).UtcDateTime;
                        System.Console.WriteLine($"   {time:yyyy-MM-dd HH:mm} UTC - O:{candle.Open} H:{candle.High} L:{candle.Low} C:{candle.Close} V:{candle.BaseVolume}");
                    }
                    else
                    {
                        System.Console.WriteLine($"   INVALID TIMESTAMP - O:{candle.Open} H:{candle.High} L:{candle.Low} C:{candle.Close} V:{candle.BaseVolume}");
                    }
                }
            }
            else
            {
                System.Console.WriteLine($"   ✗ Error: {candlesResponse.Message}");
            }
            System.Console.WriteLine();

            System.Console.WriteLine("✓ Advanced public API demo completed!");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"✗ Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                System.Console.WriteLine($"   Inner: {ex.InnerException.Message}");
            }
        }
    }
}
