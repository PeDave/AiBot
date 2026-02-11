# Quick Start Guide - BitgetApi.Dashboard

## Installation (5 minutes)

### Step 1: Prerequisites
```bash
# Verify .NET 8.0 is installed
dotnet --version
# Should show 8.0.x or later
```

If not installed, download from: https://dotnet.microsoft.com/download/dotnet/8.0

### Step 2: Clone and Setup
```bash
# Clone the repository
git clone https://github.com/PeDave/AiBot.git
cd AiBot/BitgetApi.Dashboard

# Create configuration file
cp appsettings.json.example appsettings.json

# Optional: Edit appsettings.json to customize symbols
# Default symbols: BTCUSDT, ETHUSDT, XRPUSDT
```

### Step 3: Build and Run
```bash
# Restore dependencies and build
dotnet build

# Run the dashboard
dotnet run
```

## First Run

When you start the dashboard, you'll see:

1. **Welcome Banner**: Bitget Dashboard ASCII art logo
2. **Initialization**: WebSocket connections being established
3. **Dashboard**: Live updating panels with market data

The dashboard will show:
- **PRICES**: Real-time prices for configured symbols with 24h change %
- **ORDER BOOK**: Top 5 bids and asks with visual depth bars
- **TRADES**: Last 10 trades (BUY in green, SELL in red)
- **STATS**: Uptime, messages received, updates/second, connection status

## Keyboard Commands

- **Q** or **Esc**: Quit the dashboard
- **C**: Clear trade history
- **R**: Reconnect WebSocket (if connection is lost)

## Customization

Edit `appsettings.json`:

```json
{
  "Dashboard": {
    "Symbols": ["BTCUSDT", "SOLUSDT", "ADAUSDT"],  // Change symbols here
    "RefreshRateMs": 200,                           // Slower refresh = less CPU
    "OrderBookDepth": 5,                            // 5 or 15 levels
    "TradeHistorySize": 15                          // Number of trades to show
  }
}
```

## Troubleshooting

### "WebSocket connection failed"
- Check internet connection
- Try reconnecting with **R** key
- Verify Bitget API is accessible

### Display looks wrong
- Maximize terminal window
- Use a modern terminal (Windows Terminal, iTerm2, etc.)
- Ensure terminal supports Unicode

### High CPU usage
- Increase `RefreshRateMs` to 200-500ms
- Reduce number of symbols

## Tips

1. **Terminal Size**: Use at least 120x40 terminal for best display
2. **Performance**: Default 100ms refresh is very smooth but uses more CPU
3. **Multiple Symbols**: Track up to 5 symbols for optimal performance
4. **Dark Theme**: Dashboard looks best on dark terminal backgrounds

## Next Steps

- Read full [README.md](README.md) for advanced features
- Explore bonus features: Price alerts, CSV export
- Customize the UI by modifying `UI/DashboardRenderer.cs`

## Support

Questions or issues? Open an issue on GitHub: https://github.com/PeDave/AiBot/issues

---

Happy trading! 📈
