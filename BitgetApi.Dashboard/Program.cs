using System;
using System.Collections.Generic;
using BitgetApi;
using BitgetApi.Dashboard.Services;
using BitgetApi.Dashboard.UI;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

// Build configuration
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

// Get configuration values
var symbols = config.GetSection("Dashboard:Symbols").Get<string[]>() ?? new[] { "BTCUSDT", "ETHUSDT", "XRPUSDT" };
var refreshRateMs = config.GetValue<int>("Dashboard:RefreshRateMs", 100);
var orderBookDepth = config.GetValue<int>("Dashboard:OrderBookDepth", 5);
var tradeHistorySize = config.GetValue<int>("Dashboard:TradeHistorySize", 20);

// Show welcome message
AnsiConsole.Write(new FigletText("Bitget Dashboard")
    .Centered()
    .Color(Color.Cyan1));

AnsiConsole.MarkupLine("[dim]Initializing WebSocket connections...[/]");

// Initialize services
var client = new BitgetApiClient();
var priceTracker = new PriceTrackerService();
var orderBook = new OrderBookService();
var tradeStream = new TradeStreamService(tradeHistorySize);
var stats = new PerformanceMonitor();
var renderer = new DashboardRenderer();

// Wire up event handlers for statistics
priceTracker.OnPriceUpdated += (symbol, price) => stats.RecordMessage();
orderBook.OnOrderBookUpdated += snapshot => stats.RecordMessage();
tradeStream.OnTradeReceived += trade => stats.RecordMessage();

try
{
    // ✅ CONNECT WebSocket FIRST!
    AnsiConsole.MarkupLine("[yellow]Connecting to Bitget WebSocket...[/]");
    await client.WebSocket.ConnectPublicAsync();
    AnsiConsole.MarkupLine("[green]✓[/] Connected to WebSocket!");
    await Task.Delay(1000); // Give it time to stabilize

    // Subscribe to all symbols
    foreach (var symbol in symbols)
    {
        await priceTracker.SubscribeAsync(symbol, client);
        AnsiConsole.MarkupLine($"[green]✓[/] Subscribed to {symbol} ticker");
    }

    // Subscribe to order book for the first symbol
    await orderBook.SubscribeAsync(symbols[0], client, orderBookDepth);
    AnsiConsole.MarkupLine($"[green]✓[/] Subscribed to {symbols[0]} order book");

    // Subscribe to trades for the first symbol
    await tradeStream.SubscribeAsync(symbols[0], client);
    AnsiConsole.MarkupLine($"[green]✓[/] Subscribed to {symbols[0]} trades");

    AnsiConsole.MarkupLine("\n[green]Dashboard is ready! Press Q to quit.[/]");
    await Task.Delay(5000);

    // Render loop with cancellation token
    var cts = new CancellationTokenSource();
    Console.CancelKeyPress += (s, e) =>
    {
        e.Cancel = true;
        cts.Cancel();
    };

    // Start render task
    var renderTask = Task.Run(async () =>
    {
        while (!cts.Token.IsCancellationRequested)
        {
            try
            {
                renderer.Render(priceTracker, orderBook, tradeStream, stats, symbols);
                await Task.Delay(refreshRateMs, cts.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }, cts.Token);

    // Keyboard input handler
    var inputTask = Task.Run(() =>
    {
        while (!cts.Token.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                
                if (key == ConsoleKey.Q || key == ConsoleKey.Escape)
                {
                    cts.Cancel();
                    break;
                }
                else if (key == ConsoleKey.C)
                {
                    tradeStream.Clear();
                }
            }
            
            Thread.Sleep(50);
        }
    }, cts.Token);

    // Wait for either task to complete
    await Task.WhenAny(renderTask, inputTask);
    cts.Cancel();

    // Wait for both tasks to complete
    try
    {
        await Task.WhenAll(renderTask, inputTask);
    }
    catch (OperationCanceledException)
    {
        // Expected when cancelling
    }
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
}
finally
{
    // Cleanup
    client.Dispose();
    
    AnsiConsole.Clear();
    AnsiConsole.MarkupLine("[cyan]Dashboard stopped. Thank you for using Bitget Dashboard![/]");
}
