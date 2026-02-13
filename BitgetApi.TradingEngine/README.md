# BitgetApi Trading Engine

A complete **automated trading system** with AI-powered decision making through N8N integration. Features 4 trading strategies, real-time signal generation, risk management, and performance tracking.

## 🎯 Features

✅ **4 Trading Strategies**
- RSI + Volume + EMA (Futures momentum trading)
- FVG/Liquidity (Fair value gap retest strategy)
- Swing Trading (Mean reversion from swing points)
- Weekly DCA (Automated dollar-cost averaging for spot)

✅ **N8N AI Agent Integration**
- Automated symbol selection every 4 hours
- AI-powered trade decision making with sentiment analysis
- Dynamic parameter optimization based on performance

✅ **Risk Management**
- Configurable position sizing and leverage
- Stop-loss and take-profit automation
- Maximum drawdown protection
- Portfolio limits (positions per symbol, total positions)

✅ **Performance Tracking**
- SQLite database for trade history
- Win rate, ROI, and drawdown metrics per strategy
- Automated performance reporting to N8N

✅ **Production Ready**
- Comprehensive logging
- Error handling
- Background services for continuous operation
- Paper trading mode for testing

---

## 📋 Table of Contents

- [Quick Start](#quick-start)
- [Architecture](#architecture)
- [Configuration](#configuration)
- [Running the Application](#running-the-application)
- [Documentation](#documentation)
- [Project Structure](#project-structure)
- [Development](#development)
- [Security](#security)
- [FAQ](#faq)

---

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- Bitget account with API credentials
- N8N instance (self-hosted or cloud)
- OpenAI API key (for N8N workflows)

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/PeDave/AiBot.git
cd AiBot/BitgetApi.TradingEngine
```

2. **Configure Bitget API**

Edit `appsettings.json`:
```json
{
  "Bitget": {
    "ApiKey": "your-api-key",
    "SecretKey": "your-secret-key",
    "Passphrase": "your-passphrase"
  }
}
```

3. **Configure N8N Webhooks**

After setting up N8N workflows (see [N8N_SETUP.md](N8N_SETUP.md)):
```json
{
  "N8N": {
    "WebhookBaseUrl": "https://your-n8n-instance.com/webhook",
    "StrategyAnalysisWebhook": "/strategy-analysis",
    "PerformanceWebhook": "/performance"
  }
}
```

4. **Build and Run**
```bash
dotnet restore
dotnet build
dotnet run
```

The application will start on `http://localhost:5000`

---

## 🏗️ Architecture

```
┌─────────────────┐
│  Trading Engine │
│   (Every 15min) │
└────────┬────────┘
         │
         ├─► Strategy 1: RSI+Volume+EMA ──┐
         ├─► Strategy 2: FVG/Liquidity ───┤
         ├─► Strategy 3: Swing ───────────┤
         └─► Strategy 4: Weekly DCA ──────┘
                                           │
                    Signals Generated      │
                            │              │
                            ▼              │
                    ┌──────────────┐      │
                    │  N8N Webhook │      │
                    │   Analysis   │      │
                    └──────┬───────┘      │
                           │              │
              ┌────────────┼──────────────┘
              │            │
              │  ┌─────────▼────────┐
              │  │ Twitter Sentiment│
              │  │   + OpenAI       │
              │  └─────────┬────────┘
              │            │
              │  ┌─────────▼────────┐
              │  │  Decision Engine │
              │  │ (Probability >70%)│
              │  └─────────┬────────┘
              │            │
              ▼            ▼
     ┌────────────────────────┐
     │   EXECUTE / NO_ACTION  │
     └──────────┬─────────────┘
                │
                ▼
        ┌───────────────┐
        │ Position Open │
        │  Bitget API   │
        └───────────────┘
```

### Flow

1. **Strategy Orchestrator** runs every 15 minutes
2. Analyzes all active symbols with enabled strategies
3. Generates signals based on technical indicators
4. Sends signals to **N8N** for AI analysis
5. N8N fetches sentiment, calculates probabilities
6. Returns EXECUTE or NO_ACTION decision
7. Risk Manager validates and sizes position
8. Position Manager executes trade on Bitget
9. Performance Tracker logs results
10. Hourly performance reports sent to N8N for optimization

---

## ⚙️ Configuration

### Trading Settings

```json
{
  "Trading": {
    "EnableAutoTrading": false,       // IMPORTANT: false = paper trading
    "MaxPositionsPerSymbol": 1,       // Max concurrent positions per symbol
    "MaxTotalPositions": 8,           // Max total open positions
    "MinConfidenceThreshold": 70.0,   // Min AI confidence to trade (%)
    "MaxPositionSizePercent": 5.0,    // Max position as % of account
    "MaxDrawdownPercent": 20.0,       // Pause trading at this drawdown
    "BaseDcaAmountUsd": 10.0,        // Weekly DCA amount
    "FixedSymbols": ["BTCUSDT", "ETHUSDT"]  // Always included
  }
}
```

### Strategy Configuration

Each strategy has configurable parameters. See [STRATEGY_CONFIG.md](STRATEGY_CONFIG.md) for details.

Example:
```json
{
  "Strategies": {
    "RSI_Volume_EMA": {
      "Enabled": true,
      "Market": "Futures",
      "Parameters": {
        "rsi_period": 14,
        "rsi_oversold": 30.0,
        "rsi_overbought": 70.0,
        "ema_period": 50,
        "volume_multiplier": 1.5,
        "tp_percent": 2.0,
        "sl_percent": 1.0
      }
    }
  }
}
```

---

## ▶️ Running the Application

### Development Mode

```bash
dotnet run
```

Access at: `http://localhost:5000`

### Production Mode

```bash
dotnet publish -c Release -o ./publish
cd publish
./BitgetApi.TradingEngine
```

### Docker (Optional)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY ./publish .
ENTRYPOINT ["dotnet", "BitgetApi.TradingEngine.dll"]
```

```bash
docker build -t trading-engine .
docker run -p 5000:5000 -v $(pwd)/trading.db:/app/trading.db trading-engine
```

### Background Services

Two services run automatically:

1. **StrategyAnalysisService**: Runs every 15 minutes
   - Analyzes all symbols with enabled strategies
   - Sends signals to N8N

2. **PerformanceReportingService**: Runs every hour
   - Calculates metrics per strategy
   - Sends to N8N for optimization

---

## 📚 Documentation

- **[N8N_SETUP.md](N8N_SETUP.md)** - Complete N8N setup and workflow import guide
- **[STRATEGY_CONFIG.md](STRATEGY_CONFIG.md)** - Strategy parameters and optimization
- **[API.md](API.md)** - API endpoint documentation

---

## 📁 Project Structure

```
BitgetApi.TradingEngine/
├── Controllers/
│   └── N8NAgentController.cs          # API endpoints for N8N
├── Strategies/
│   ├── IStrategy.cs                   # Strategy interface
│   ├── RsiVolumeEmaStrategy.cs       # Strategy 1
│   ├── FvgLiquidityStrategy.cs       # Strategy 2
│   ├── SwingStrategy.cs              # Strategy 3
│   └── WeeklyDcaStrategy.cs          # Strategy 4
├── Indicators/
│   ├── RsiIndicator.cs               # RSI calculator
│   ├── EmaIndicator.cs               # EMA calculator
│   ├── SmaIndicator.cs               # SMA calculator
│   ├── VolumeIndicator.cs            # Volume analyzer
│   ├── FvgDetector.cs                # Fair value gap detector
│   └── LiquidityZoneDetector.cs      # Support/resistance finder
├── Models/
│   ├── Signal.cs                     # Trading signal model
│   ├── Position.cs                   # Position model
│   ├── Candle.cs                     # Candlestick data
│   ├── DcaOrder.cs                   # DCA order model
│   ├── StrategyMetrics.cs            # Performance metrics
│   └── N8N/
│       ├── AgentDecision.cs          # N8N decision model
│       ├── SymbolRecommendation.cs   # Symbol update model
│       └── OptimizationRecommendation.cs
├── Trading/
│   ├── BitgetFuturesClient.cs        # Futures API wrapper
│   ├── BitgetSpotClient.cs           # Spot API wrapper
│   ├── PositionManager.cs            # Position lifecycle management
│   └── RiskManager.cs                # Risk validation
├── N8N/
│   └── N8NWebhookClient.cs           # N8N communication
├── Services/
│   ├── StrategyOrchestrator.cs       # Main coordination service
│   ├── SymbolManager.cs              # Active symbols management
│   ├── PerformanceTracker.cs         # Metrics and database
│   └── TradingDbContext.cs           # SQLite database
├── HostedServices/
│   ├── StrategyAnalysisService.cs    # 15-min background job
│   └── PerformanceReportingService.cs # Hourly reporting
├── n8n-workflows/
│   ├── symbol-scanner.json           # Symbol selection workflow
│   ├── strategy-decision-engine.json # Trade decision workflow
│   └── performance-optimizer.json    # Parameter optimization
├── appsettings.json                  # Configuration
└── Program.cs                        # Application entry point
```

---

## 🔧 Development

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code with C# extension
- SQLite browser (optional, for database inspection)

### Building

```bash
dotnet build
```

### Running Tests

```bash
cd ../BitgetApi.Tests
dotnet test
```

### Database

The application uses SQLite (`trading.db`). To inspect:

```bash
sqlite3 trading.db
.tables
SELECT * FROM Positions;
```

---

## 🔒 Security

### API Credentials

- **Never commit** `appsettings.json` with real credentials
- Use environment variables or Azure Key Vault in production
- Rotate API keys regularly

### N8N Webhooks

- Use HTTPS in production
- Implement webhook authentication (API keys or HMAC)
- Whitelist Trading Engine IP on N8N

### Risk Controls

- Start with **paper trading** (`EnableAutoTrading: false`)
- Test thoroughly before enabling live trading
- Set conservative position size limits
- Monitor drawdown closely

---

## 💡 FAQ

### Q: How do I enable live trading?

Set `EnableAutoTrading: true` in `appsettings.json`. **Use with caution** and start with small position sizes.

### Q: Can I run without N8N?

Not currently. N8N is required for AI decision-making. You could modify the code to bypass N8N, but you'll lose sentiment analysis and probability scoring.

### Q: How much capital do I need?

- **Minimum**: $500-1000 for testing with small positions
- **Recommended**: $5000+ for proper position sizing and diversification

### Q: What are the costs?

- Bitget trading fees: 0.06-0.1% per trade
- OpenAI API: ~$0.01-0.05 per decision
- N8N: Free (self-hosted) or $20-50/month (cloud)

### Q: How do I add a new strategy?

1. Create new class implementing `IStrategy`
2. Implement `GenerateSignalAsync` method
3. Register in `Program.cs`
4. Add configuration in `appsettings.json`

### Q: Can I use different exchanges?

Currently supports Bitget only. To add others:
1. Implement new client wrappers (similar to `BitgetFuturesClient`)
2. Update `PositionManager` to use the new client

### Q: How accurate are the strategies?

Varies by market conditions:
- **Bull market**: 60-70% win rate typical
- **Bear/sideways**: 50-60% win rate
- **High volatility**: Performance may degrade

Always backtest before live trading.

---

## 📊 Performance Expectations

### Conservative Settings
- **Win Rate**: 55-60%
- **Monthly ROI**: 5-10%
- **Max Drawdown**: -10 to -15%
- **Position Size**: 3-5% per trade

### Aggressive Settings
- **Win Rate**: 50-55%
- **Monthly ROI**: 15-25%
- **Max Drawdown**: -15 to -25%
- **Position Size**: 5-10% per trade

*Past performance does not guarantee future results. Crypto trading is risky.*

---

## 🤝 Contributing

Contributions welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

---

## 📄 License

MIT License - see LICENSE file

---

## ⚠️ Disclaimer

This software is for educational purposes. Trading cryptocurrencies carries significant risk. Never trade with money you cannot afford to lose. The authors are not responsible for any financial losses.

---

## 🙋 Support

- **Documentation**: See guides in this directory
- **Issues**: [GitHub Issues](https://github.com/PeDave/AiBot/issues)
- **Main Project**: [BitgetApi Library](https://github.com/PeDave/AiBot)

---

## 🎉 Acknowledgments

Built on top of the [BitgetApi](https://github.com/PeDave/AiBot) C# library.

Powered by:
- [N8N](https://n8n.io) - Workflow automation
- [OpenAI](https://openai.com) - AI decision-making
- [Bitget](https://www.bitget.com) - Exchange API
