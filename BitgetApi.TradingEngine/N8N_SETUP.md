# N8N Setup Guide

## 🏗️ Architecture Overview

**New Architecture (C# API Master + N8N AI Brain):**

```
┌──────────────────────────────────────────────┐
│  C# Trading Engine (API Master)              │
│  ✅ All Bitget API calls                      │
│  ✅ Symbol scanning                           │
│  ✅ Strategy analysis                         │
│  ✅ Market data fetching                      │
│  ✅ Performance tracking                      │
└────────────────┬─────────────────────────────┘
                 │ REST API
                 │ (No rate limits, no external APIs)
┌────────────────▼─────────────────────────────┐
│  N8N Workflows (AI Brain)                    │
│  ✅ Decision making                           │
│  ✅ Sentiment analysis (optional)            │
│  ✅ Strategy optimization                    │
│  ✅ Trade execution decisions                │
└──────────────────────────────────────────────┘
```

**Benefits:**
- ✅ No CoinGecko API rate limits (free tier restrictions removed)
- ✅ No external API dependencies in N8N
- ✅ Clean separation of concerns
- ✅ Centralized Bitget API management in C#
- ✅ N8N focuses on AI/decision logic only

---

## 📡 Available API Endpoints

The C# Trading Engine exposes the following REST API endpoints for N8N:

### Symbol Scanning
```
GET /api/symbols/scan?count=10
```
Returns top symbols by volume and volatility from Bitget.

**Response:**
```json
{
  "symbols": [
    {
      "symbol": "BTCUSDT",
      "score": 8.5,
      "volume24h": 25000000000,
      "priceChange24h": 3.2,
      "price": 66500,
      "high24h": 67200,
      "low24h": 65800
    }
  ],
  "count": 10,
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Strategy Analysis
```
POST /api/strategies/analyze
Content-Type: application/json

{
  "symbols": ["BTCUSDT", "ETHUSDT"]
}
```
Analyzes symbols with all enabled strategies and returns signals.

**Response:**
```json
{
  "analyses": [
    {
      "symbol": "BTCUSDT",
      "signals": [
        {
          "strategy": "RSI_Volume_EMA",
          "type": "Long",
          "entryPrice": 66500,
          "stopLoss": 65835,
          "takeProfit": 67830,
          "confidence": 72.5,
          "reason": "RSI oversold + volume spike",
          "timestamp": "2024-01-15T10:30:00Z"
        }
      ]
    }
  ],
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Performance Metrics
```
GET /api/performance
```
Returns performance metrics for all strategies.

**Response:**
```json
{
  "strategies": [
    {
      "name": "RSI_Volume_EMA",
      "winRate": 62.5,
      "roi": 15.3,
      "drawdown": -8.2,
      "totalTrades": 45,
      "avgWin": 3.2,
      "avgLoss": -1.8,
      "parameters": {
        "rsi_oversold": 30,
        "volume_multiplier": 1.5
      }
    }
  ],
  "overallPerformance": {
    "totalRoi": 28.5,
    "totalTrades": 120,
    "avgWinRate": 58.3,
    "maxDrawdown": -12.5
  },
  "openPositions": 3,
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### Market Data - Candles
```
GET /api/market/{symbol}/candles?timeframe=1h&limit=200
```
Returns candle data for a symbol.

### Market Data - Ticker
```
GET /api/market/{symbol}/ticker
```
Returns current ticker data for a symbol.

---

## Prerequisites

- N8N instance (self-hosted or cloud)
- C# Trading Engine running (default: http://localhost:5000)
- OpenAI API key (optional, for AI-enhanced decision making)
- **No CoinGecko API needed anymore!** ✅

## Installation Steps

### 1. Install N8N

**Option A: Docker (Recommended)**
```bash
docker run -it --rm \
  --name n8n \
  -p 5678:5678 \
  -v ~/.n8n:/home/node/.n8n \
  n8nio/n8n
```

**Option B: npm**
```bash
npm install n8n -g
n8n start
```

Access N8N at: `http://localhost:5678`

### 2. Configure Credentials

#### OpenAI Credentials
1. Go to Settings > Credentials
2. Click "+ Add Credential"
3. Search for "OpenAI"
4. Enter your OpenAI API key
5. Save

#### Twitter/X API (Optional)
1. Create Twitter Developer Account at https://developer.twitter.com
2. Create a new app and get Bearer Token
3. In N8N: Settings > Credentials > OAuth2 API
4. Configure with Twitter API credentials

#### CoinGecko API
No credentials needed for free tier.

### 3. Import Workflows

#### Workflow 1: Symbol Scanner + Strategy Analysis (NEW)
This is the main workflow that replaces the old symbol-scanner and strategy-decision-engine workflows.

1. Open N8N
2. Click "Workflows" > "Import from File"
3. Select `n8n-workflows/symbol-scanner-strategy-analysis.json`
4. Click "Import"
5. **Configure**:
   - Update all HTTP Request nodes to point to your Trading Engine URL (default: http://localhost:5000)
   - The workflow runs every 15 minutes automatically
   - No external API credentials needed!

**What it does:**
- Fetches top 10 symbols from C# API (`/api/symbols/scan`)
- Filters to top 6 (always includes BTC and ETH)
- Analyzes strategies via C# API (`/api/strategies/analyze`)
- Adds sentiment score (mock or real if you integrate sentiment API)
- Calculates trade decision based on multiple signals
- Sends decision back to C# API (`/api/n8n/decision`)

#### Workflow 2: Performance Optimizer (NEW API)
1. Import `n8n-workflows/performance-optimizer-new-api.json`
2. **Configure**:
   - Update HTTP Request nodes to your Trading Engine URL
   - Runs every 6 hours automatically

**What it does:**
- Fetches performance metrics from C# API (`/api/performance`)
- Analyzes win rates, ROI, and drawdown
- Generates parameter optimization suggestions
- Sends optimizations to C# API (`/api/n8n/optimize`)

#### Legacy Workflows (Optional)
The old workflow files are still available if needed:
- `symbol-scanner.json` - Old version using CoinGecko API
- `strategy-decision-engine.json` - Old version with webhook trigger
- `performance-optimizer.json` - Old version with webhook trigger

**Recommendation:** Use the new workflows above for the clean API architecture.

### 4. Activate Workflows

1. Open each workflow
2. Click the toggle switch at top right to activate
3. Symbol Scanner will run every 4 hours automatically
4. Other workflows are webhook-triggered

## Testing Workflows

### Test Symbol Scanner + Strategy Analysis
1. Open the workflow in N8N
2. Click "Execute Workflow" button
3. Check execution log for:
   - Symbol scan results
   - Strategy analysis results
   - Decision output
4. Verify Trading Engine received decision at `/api/n8n/decision`

### Test Performance Optimizer
1. Make sure you have some trade history in the database
2. Open the workflow
3. Click "Execute Workflow"
4. Check execution log for optimization suggestions
5. Verify Trading Engine received optimizations at `/api/n8n/optimize`

### Test C# API Endpoints Directly

You can test the API endpoints using curl or Swagger UI:

**Swagger UI:** Navigate to `http://localhost:5000/swagger` when Trading Engine is running

**Test Symbol Scan:**
```bash
curl http://localhost:5000/api/symbols/scan?count=10
```

**Test Strategy Analysis:**
```bash
curl -X POST http://localhost:5000/api/strategies/analyze \
  -H "Content-Type: application/json" \
  -d '{"symbols": ["BTCUSDT", "ETHUSDT"]}'
```

**Test Performance Metrics:**
```bash
curl http://localhost:5000/api/performance
```

**Test Market Data:**
```bash
curl http://localhost:5000/api/market/BTCUSDT/ticker
curl http://localhost:5000/api/market/BTCUSDT/candles?timeframe=1h&limit=100
```

## Workflow Details

### Symbol Scanner + Strategy Analysis (NEW)
- **Runs**: Every 15 minutes (configurable)
- **Purpose**: Complete symbol selection and strategy analysis pipeline
- **Data Flow**:
  1. C# scans Bitget for top symbols by volume/volatility
  2. N8N filters to top 6 (includes BTC/ETH)
  3. C# analyzes each symbol with all enabled strategies
  4. N8N adds sentiment scores (optional)
  5. N8N calculates weighted decision from multiple signals
  6. C# executes trade if decision is EXECUTE
- **Output**: Trade execution via C# Trading Engine

### Performance Optimizer (NEW)
- **Runs**: Every 6 hours (configurable)
- **Purpose**: Analyze strategy performance and optimize parameters
- **Data Flow**:
  1. C# provides all strategy metrics
  2. N8N analyzes win rates, ROI, drawdown
  3. N8N generates optimization suggestions
  4. C# applies high-confidence optimizations
- **Output**: Updated strategy parameters

### Legacy Workflows (If Still Using)
See backup documentation in `n8n-workflows/*.json.backup` files.

## Troubleshooting

### Workflow not executing
- Check if workflow is activated (toggle switch)
- Verify C# Trading Engine is running at configured URL
- Check N8N logs for errors
- Test API endpoints directly with curl

### Cannot connect to Trading Engine API
- Verify Trading Engine is running: `dotnet run --project BitgetApi.TradingEngine`
- Check the URL in HTTP Request nodes (default: http://localhost:5000)
- Ensure firewall allows connections
- Check Trading Engine logs for errors

### No symbols returned from scan
- Verify Bitget API is accessible
- Check Trading Engine logs for API errors
- Test endpoint directly: `curl http://localhost:5000/api/symbols/scan`

### Strategy analysis returns no signals
- Ensure strategies are enabled in `appsettings.json`
- Check if symbols have sufficient candle data (need 250+ candles)
- Verify strategy parameters are not too restrictive
- Check Trading Engine logs for analysis errors

### Decision not executing trades
- Verify `Trading:EnableAutoTrading` is `true` in `appsettings.json`
- Check risk management settings (max positions, etc.)
- Review Trading Engine logs for validation failures
- Ensure Bitget API credentials are valid

### Performance metrics empty
- Make sure you have closed positions in the database
- Check if strategies have been running long enough
- Verify database connection in Trading Engine

## Production Deployment

### C# Trading Engine
1. Deploy Trading Engine to a server or cloud (Azure, AWS, etc.)
2. Update `appsettings.json` with production settings
3. Configure HTTPS and domain name
4. Set up monitoring and logging

### N8N Configuration
1. Update all HTTP Request node URLs to production Trading Engine URL
2. Use HTTPS for all API calls
3. Consider adding API authentication headers
4. Set up error notifications (email/Slack/Discord)

### Security Best Practices
1. **API Authentication**: Add API key authentication to Trading Engine endpoints
   ```csharp
   // In Program.cs
   builder.Services.AddAuthentication("ApiKey")
       .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>("ApiKey", null);
   ```
2. **Rate Limiting**: Add rate limiting to prevent abuse
3. **HTTPS Only**: Use TLS/SSL certificates for all API communication
4. **Network Security**: Whitelist N8N IP addresses in firewall
5. **Secrets Management**: Use environment variables or Azure Key Vault for sensitive data

### Monitoring
1. Enable Application Insights or similar monitoring
2. Set up alerts for API failures
3. Monitor Trading Engine performance and memory usage
4. Track N8N workflow execution success rates
5. Monitor Bitget API rate limits and quotas

### Scaling
1. Use horizontal scaling for Trading Engine if needed
2. Consider Redis cache for frequently accessed data
3. Use N8N queue mode for high-volume workflows
4. Optimize database queries and indexing

## Support

For N8N issues:
- Documentation: https://docs.n8n.io
- Community: https://community.n8n.io

For Trading Engine issues:
- See `README.md` in project root
- Check `API.md` for endpoint documentation
