using BitgetApi;
using BitgetApi.Models;

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
        await DemoPublicApiAsync();

        System.Console.WriteLine();
        System.Console.WriteLine("Press any key to continue to authenticated examples (requires API keys)...");
        System.Console.ReadKey();
        System.Console.WriteLine();

        // Demo 2: Private API - Authentication required
        // Uncomment and add your credentials to test authenticated endpoints
        // await DemoPrivateApiAsync();

        // Demo 3: WebSocket - Real-time data
        // await DemoWebSocketAsync();

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
                var depth = depthResponse.Data;
                System.Console.WriteLine("   Asks (Sell Orders):");
                foreach (var ask in depth.Asks.Take(5))
                {
                    System.Console.WriteLine($"      Price: {ask.Price}, Size: {ask.Size}");
                }
                System.Console.WriteLine("   Bids (Buy Orders):");
                foreach (var bid in depth.Bids.Take(5))
                {
                    System.Console.WriteLine($"      Price: {bid.Price}, Size: {bid.Size}");
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

        // TODO: Replace with your actual API credentials
        var credentials = new BitgetCredentials
        {
            ApiKey = "YOUR_API_KEY",
            SecretKey = "YOUR_SECRET_KEY",
            Passphrase = "YOUR_PASSPHRASE"
        };

        using var client = new BitgetApiClient(credentials);

        try
        {
            // Get spot account assets
            System.Console.WriteLine("1. Getting spot account assets...");
            var assetsResponse = await client.SpotAccount.GetAccountAssetsAsync();
            if (assetsResponse.IsSuccess && assetsResponse.Data != null)
            {
                foreach (var asset in assetsResponse.Data.Where(a => decimal.Parse(a.Available) > 0).Take(5))
                {
                    System.Console.WriteLine($"   {asset.Coin}: Available={asset.Available}, Frozen={asset.Frozen}");
                }
            }
            System.Console.WriteLine();

            // Get open orders
            System.Console.WriteLine("2. Getting open spot orders...");
            var ordersResponse = await client.SpotTrade.GetOpenOrdersAsync();
            if (ordersResponse.IsSuccess && ordersResponse.Data != null)
            {
                System.Console.WriteLine($"   Total Open Orders: {ordersResponse.Data.Count}");
                foreach (var order in ordersResponse.Data.Take(5))
                {
                    System.Console.WriteLine($"   - {order.Symbol}: {order.Side} {order.Size} @ {order.Price}");
                }
            }
            System.Console.WriteLine();

            // Get futures account
            System.Console.WriteLine("3. Getting futures account (USDT)...");
            var futuresAccountResponse = await client.FuturesAccount.GetAccountsAsync("USDT-FUTURES");
            if (futuresAccountResponse.IsSuccess && futuresAccountResponse.Data != null)
            {
                foreach (var account in futuresAccountResponse.Data)
                {
                    System.Console.WriteLine($"   {account.MarginCoin}: Available={account.Available}, Equity={account.Equity}");
                }
            }
            System.Console.WriteLine();

            System.Console.WriteLine("✓ Private API demo completed successfully!");
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"✗ Error: {ex.Message}");
            System.Console.WriteLine("   Note: Make sure to set valid API credentials");
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
}
