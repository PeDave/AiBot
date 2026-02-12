using BitgetApi.Dashboard.Models;
using BitgetApi.Dashboard.Services;
using Spectre.Console;
using Microsoft.Extensions.Logging;

namespace BitgetApi.Dashboard.UI;

public class DashboardRenderer
{
    private readonly Layout _layout;
    private readonly ILogger<DashboardRenderer>? _logger;
    
    public DashboardRenderer(ILogger<DashboardRenderer>? logger = null)
    {
        _logger = logger;
        _layout = new Layout("Root")
            .SplitRows(
                new Layout("Header").Size(3),
                new Layout("Content").SplitColumns(
                    new Layout("Left").Ratio(1),
                    new Layout("Right").Ratio(2)
                ),
                new Layout("Footer").Size(3)
            );
        
        // Split the Left column into Prices and Trades
        _layout["Content"]["Left"].SplitRows(
            new Layout("Prices").Ratio(1),
            new Layout("Trades").Ratio(2)
        );
        
        // Split the Right column into OrderBook and Stats
        _layout["Content"]["Right"].SplitRows(
            new Layout("OrderBook").Ratio(3),
            new Layout("Stats").Ratio(1)
        );
    }
    
    public void Render(
        PriceTrackerService priceTracker, 
        OrderBookService orderBook, 
        TradeStreamService tradeStream, 
        PerformanceMonitor stats,
        string[] symbols)
    {
        _layout["Header"].Update(CreateHeader());
        _layout["Content"]["Left"]["Prices"].Update(CreatePricesPanel(priceTracker, symbols));
        _layout["Content"]["Left"]["Trades"].Update(CreateTradesPanel(tradeStream));
        _layout["Content"]["Right"]["OrderBook"].Update(CreateOrderBookPanel(orderBook));
        _layout["Content"]["Right"]["Stats"].Update(CreateStatsPanel(stats));
        _layout["Footer"].Update(CreateFooter());
        
        AnsiConsole.Clear();
        AnsiConsole.Write(_layout);
    }
    
    private Panel CreateHeader()
    {
        var text = new Markup("[bold cyan]BITGET LIVE MARKET DASHBOARD v1.0[/]");
        return new Panel(Align.Center(text))
            .BorderColor(Color.Cyan1)
            .Padding(0, 0);
    }

    private Panel CreatePricesPanel(PriceTrackerService priceTracker, string[] symbols)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .HideHeaders();

        table.AddColumn("");
        table.AddColumn("");
        table.AddColumn("");

        foreach (var symbol in symbols)
        {
            var price = priceTracker.GetPrice(symbol);

            _logger?.LogDebug("DashboardRenderer: GetPrice({Symbol}) = {Price}", 
                symbol, price == null ? "NULL" : $"${price.LastPrice}");

            if (price != null)
            {
                var changeColor = price.Change24h >= 0 ? "green" : "red";
                var arrow = price.Change24h >= 0 ? "▲" : "▼";
                var symbolDisplay = symbol.Replace("USDT", "");

                table.AddRow(
                    $"[bold]{symbolDisplay}[/]",
                    $"[bold]${price.LastPrice:N2}[/]",
                    $"[{changeColor}]{arrow}{Math.Abs(price.Change24h):F2}%[/]"
                );
            }
            else
            {
                var symbolDisplay = symbol.Replace("USDT", "");
                table.AddRow($"[bold]{symbolDisplay}[/]", "[dim]...[/]", "[dim]...[/]");
            }
        }

        return new Panel(table)
            .Header("[yellow]PRICES[/]")
            .BorderColor(Color.Cyan1);
    }

    private Panel CreateTradesPanel(TradeStreamService tradeStream)
    {
        var trades = tradeStream.GetRecentTrades().Take(10).ToList();
        
        if (trades.Count == 0)
        {
            return new Panel("[dim]Waiting for trades...[/]")
                .Header("[yellow]RECENT TRADES[/]")
                .BorderColor(Color.Cyan1);
        }
        
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .HideHeaders();
        
        table.AddColumn("");
        table.AddColumn("");
        table.AddColumn("");
        
        foreach (var trade in trades)
        {
            var sideColor = trade.Side == "buy" ? "green" : "red";
            var sideText = trade.Side.ToUpperInvariant();
            var time = trade.Timestamp.ToString("HH:mm:ss");
            
            table.AddRow(
                $"[dim]{time}[/]",
                $"[{sideColor}]{sideText}[/]",
                $"{trade.Size:F4}"
            );
        }
        
        return new Panel(table)
            .Header("[yellow]RECENT TRADES[/]")
            .BorderColor(Color.Cyan1);
    }
    
    private Panel CreateOrderBookPanel(OrderBookService orderBook)
    {
        var snapshot = orderBook.GetSnapshot();
        if (snapshot == null)
        {
            return new Panel("[dim]Waiting for order book data...[/]")
                .Header("[yellow]ORDER BOOK[/]")
                .BorderColor(Color.Cyan1);
        }
        
        var grid = new Grid();
        grid.AddColumn(new GridColumn().Width(12));
        grid.AddColumn(new GridColumn().Width(25));
        grid.AddColumn(new GridColumn().Width(10));
        
        // ASKS (Sell orders)
        grid.AddRow(new Markup("[red bold]ASKS (SELL)[/]"), new Markup(""), new Markup(""));
        
        var asks = snapshot.Asks.Take(5).ToList();
        var maxAskSize = asks.Any() ? asks.Max(a => a.Size) : 1;
        
        foreach (var ask in asks)
        {
            var bar = CreateBar(ask.Size, maxAskSize, 20, "red");
            grid.AddRow(
                new Markup($"[red]${ask.Price:N2}[/]"),
                new Markup(bar),
                new Markup($"[dim]{ask.Size:F4}[/]")
            );
        }
        
        // Spread
        if (asks.Any() && snapshot.Bids.Any())
        {
            var spread = asks.First().Price - snapshot.Bids.First().Price;
            var spreadPct = (spread / snapshot.Bids.First().Price) * 100;
            grid.AddEmptyRow();
            grid.AddRow(
                new Markup($"[yellow]SPREAD: ${spread:F2} ({spreadPct:F3}%)[/]"),
                new Markup(""),
                new Markup("")
            );
            grid.AddEmptyRow();
        }
        
        // BIDS (Buy orders)
        grid.AddRow(new Markup("[green bold]BIDS (BUY)[/]"), new Markup(""), new Markup(""));
        
        var bids = snapshot.Bids.Take(5).ToList();
        var maxBidSize = bids.Any() ? bids.Max(b => b.Size) : 1;
        
        foreach (var bid in bids)
        {
            var bar = CreateBar(bid.Size, maxBidSize, 20, "green");
            grid.AddRow(
                new Markup($"[green]${bid.Price:N2}[/]"),
                new Markup(bar),
                new Markup($"[dim]{bid.Size:F4}[/]")
            );
        }
        
        return new Panel(grid)
            .Header($"[yellow]ORDER BOOK ({snapshot.Symbol})[/]")
            .BorderColor(Color.Cyan1);
    }
    
    private string CreateBar(decimal value, decimal max, int width, string color)
    {
        if (max == 0) return "";
        
        var filled = (int)((value / max) * width);
        var empty = width - filled;
        
        var bar = new string('█', filled) + new string('░', empty);
        return $"[{color}]{bar}[/]";
    }
    
    private Panel CreateStatsPanel(PerformanceMonitor stats)
    {
        var uptime = stats.Uptime;
        var uptimeStr = $"{uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
        
        var grid = new Grid();
        grid.AddColumn();
        grid.AddColumn();
        
        grid.AddRow(
            new Markup($"[cyan]Uptime:[/] {uptimeStr}"),
            new Markup($"[cyan]Updates/s:[/] {stats.UpdatesPerSecond:F1}")
        );
        grid.AddRow(
            new Markup($"[cyan]Messages:[/] {stats.TotalMessages:N0}"),
            new Markup($"[cyan]Status:[/] [green]{stats.GetConnectionStatus()}[/]")
        );
        
        return new Panel(grid)
            .Header("[yellow]STATISTICS[/]")
            .BorderColor(Color.Cyan1)
            .Padding(0, 0);
    }
    
    private Panel CreateFooter()
    {
        var text = new Markup("[dim]Q=Quit | R=Reconnect | C=Clear Trades | +/- Depth[/]");
        return new Panel(Align.Center(text))
            .BorderColor(Color.Grey)
            .Padding(0, 0);
    }
}
