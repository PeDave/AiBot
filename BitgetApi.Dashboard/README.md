# BitgetApi.Dashboard

A beautiful, production-ready console dashboard for real-time cryptocurrency market data visualization using Bitget WebSocket API.

![Dashboard Version](https://img.shields.io/badge/version-1.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![License](https://img.shields.io/badge/license-MIT-green)

## Features

✨ **Real-time Price Tracking** - Monitor multiple cryptocurrency pairs simultaneously  
📊 **Order Book Visualization** - ASCII progress bars showing market depth  
💹 **Live Trade Stream** - Watch trades as they happen  
📈 **Performance Metrics** - Track uptime, message rates, and connection status  
⌨️ **Keyboard Controls** - Interactive command interface  
🎨 **Beautiful Console UI** - Powered by Spectre.Console  
🔔 **Price Alerts** - Get notified when prices reach target levels (optional)  
💾 **CSV Export** - Export trade data for analysis (optional)

## Screenshot

```
┌─────────────────────────────────────────────────────────────────┐
│              BITGET LIVE MARKET DASHBOARD v1.0                  │
├─────────────────────────────────────────────────────────────────┤
│ ┌─ PRICES ─────────┐  ┌─ ORDER BOOK (BTCUSDT) ───────────────┐ │
│ │ BTC  $67,558 ▲1.2%│  │ ASKS (SELL)                          │ │
│ │ ETH  $3,245  ▼0.4%│  │ $67,560 ████████░░░░ 2.34           │ │
│ │ XRP  $0.52   ▲2.3%│  │ SPREAD: $1.00 (0.0015%)              │ │
│ └──────────────────┘  │ BIDS (BUY)                           │ │
│                       │ $67,559 ████████████ 3.45            │ │
│ ┌─ TRADES ──────────┐  └──────────────────────────────────────┘ │
│ │ 14:32 BUY  0.12   │                                           │
│ │ 14:31 SELL 0.45   │  ┌─ STATS ─────────────────────────────┐ │
│ └──────────────────┘  │ Uptime: 00:15:32 | Updates/s: 12.3  │ │
│                       │ Messages: 1,234  | Status: Connected │ │
│                       └──────────────────────────────────────┘ │
├─────────────────────────────────────────────────────────────────┤
│ Q=Quit | R=Reconnect | C=Clear Trades | +/- Depth              │
└─────────────────────────────────────────────────────────────────┘
```

## Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Terminal with Unicode support (for best visual experience)

### Installation

1. Clone the repository:
```bash
git clone https://github.com/PeDave/AiBot.git
cd AiBot/BitgetApi.Dashboard
```

2. Configure the application:
```bash
cp appsettings.json.example appsettings.json
# Edit appsettings.json with your preferences
```

3. Restore dependencies:
```bash
dotnet restore
```

4. Build the project:
```bash
dotnet build
```

5. Run the dashboard:
```bash
dotnet run
```

## Configuration

Edit `appsettings.json` to customize the dashboard:

```json
{
  "Dashboard": {
    "Symbols": ["BTCUSDT", "ETHUSDT", "XRPUSDT"],
    "RefreshRateMs": 100,
    "OrderBookDepth": 5,
    "TradeHistorySize": 20,
    "ShowAccountBalance": false,
    "EnablePriceAlerts": false,
    "ExportTradesToCsv": false
  },
  "Bitget": {
    "ApiKey": "",
    "SecretKey": "",
    "Passphrase": ""
  }
}
```

### Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| `Symbols` | Array of trading pairs to monitor | `["BTCUSDT", "ETHUSDT", "XRPUSDT"]` |
| `RefreshRateMs` | UI refresh rate in milliseconds | `100` |
| `OrderBookDepth` | Number of order book levels (5 or 15) | `5` |
| `TradeHistorySize` | Maximum trades to display | `20` |
| `ShowAccountBalance` | Show account balance (requires API keys) | `false` |
| `EnablePriceAlerts` | Enable price alert notifications | `false` |
| `ExportTradesToCsv` | Auto-export trades to CSV | `false` |

**Note:** API credentials are only required for private endpoints. Public market data works without authentication.

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| `Q` or `Esc` | Quit the dashboard |
| `R` | Reconnect WebSocket connections |
| `C` | Clear trade history |
| `+` | Increase order book depth |
| `-` | Decrease order book depth |
| `A` | Toggle price alerts |
| `E` | Export trades to CSV |

## Architecture

### Project Structure

```
BitgetApi.Dashboard/
├── Models/                      # Data models
│   ├── PriceUpdate.cs
│   ├── OrderBookSnapshot.cs
│   └── TradeRecord.cs
├── Services/                    # Business logic
│   ├── PriceTrackerService.cs
│   ├── OrderBookService.cs
│   ├── TradeStreamService.cs
│   ├── PerformanceMonitor.cs
│   ├── PriceAlertService.cs
│   └── TradeExporter.cs
├── UI/                         # User interface
│   └── DashboardRenderer.cs
├── Program.cs                  # Entry point
└── appsettings.json           # Configuration
```

### Key Components

#### Services

- **PriceTrackerService**: Subscribes to ticker updates for multiple symbols
- **OrderBookService**: Maintains real-time order book state with depth visualization
- **TradeStreamService**: Tracks recent trades in a FIFO queue
- **PerformanceMonitor**: Collects statistics (uptime, messages, updates/sec)
- **PriceAlertService**: Manages price alerts and notifications
- **TradeExporter**: Exports trade data to CSV format

#### UI

- **DashboardRenderer**: Orchestrates the console layout using Spectre.Console
  - Price panel with color-coded changes
  - Order book with ASCII progress bars
  - Trade stream with auto-scrolling
  - Statistics panel with performance metrics

## Performance

- **Render Latency**: < 50ms typical
- **Update Rate**: Configurable (default 100ms)
- **Memory Footprint**: < 50MB typical
- **WebSocket Connections**: 3-5 concurrent streams

## Troubleshooting

### Common Issues

**Issue: "WebSocket connection failed"**
- Check your internet connection
- Verify Bitget API is accessible
- Try reconnecting with `R` key

**Issue: "No data displayed"**
- Wait a few seconds for WebSocket subscriptions to initialize
- Check if the trading pairs are valid
- Verify `appsettings.json` configuration

**Issue: "Display looks broken"**
- Ensure your terminal supports Unicode
- Try maximizing the terminal window
- Use a modern terminal (Windows Terminal, iTerm2, etc.)

**Issue: "High CPU usage"**
- Increase `RefreshRateMs` in configuration (e.g., 200ms)
- Reduce number of symbols being tracked
- Close other resource-intensive applications

### Performance Tips

1. **Optimize Refresh Rate**: Set `RefreshRateMs` to 200-500ms for lower CPU usage
2. **Limit Symbols**: Track 3-5 symbols for best performance
3. **Terminal Size**: Use a larger terminal window for better layout
4. **Modern Terminal**: Use terminals with GPU acceleration (Windows Terminal, Alacritty)

## Development

### Building from Source

```bash
# Build in Debug mode
dotnet build

# Build in Release mode
dotnet build -c Release

# Run tests (if available)
dotnet test

# Publish standalone executable
dotnet publish -c Release -r win-x64 --self-contained
```

### Dependencies

- **Spectre.Console** (0.49.1): Console UI framework
- **Microsoft.Extensions.Configuration.Json** (8.0.0): Configuration management
- **Microsoft.Extensions.Hosting** (8.0.0): Application hosting
- **BitgetApi**: Core Bitget API client library

## License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.

## Acknowledgments

- Built with [Spectre.Console](https://spectreconsole.net/)
- Uses [Bitget Exchange API](https://www.bitget.com/api-doc)

## Support

For issues and feature requests, please open an issue on [GitHub](https://github.com/PeDave/AiBot/issues).

## Roadmap

- [ ] Multiple symbol order book view
- [ ] Candlestick chart visualization
- [ ] Technical indicators (RSI, MACD)
- [ ] Portfolio tracking
- [ ] Custom alert rules
- [ ] Export to multiple formats (JSON, Excel)
- [ ] Historical data playback
- [ ] Dark/Light theme toggle

---

Made with ❤️ by the BitgetApi team
