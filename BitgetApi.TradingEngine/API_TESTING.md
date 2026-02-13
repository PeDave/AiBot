# API Testing Guide

This document describes how to test the new REST API endpoints for N8N integration.

## Starting the Trading Engine

```bash
cd BitgetApi.TradingEngine
dotnet run --urls "http://localhost:5000"
```

The service will start and display:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

## Swagger UI

Navigate to: **http://localhost:5000/swagger**

This provides an interactive API documentation where you can test all endpoints.

## API Endpoints

### 1. Symbol Scanning

**GET** `/api/symbols/scan?count=10`

Returns top symbols by volume and volatility from Bitget.

**Example:**
```bash
curl "http://localhost:5000/api/symbols/scan?count=5"
```

**Response:**
```json
{
  "symbols": [
    {
      "symbol": "BTCUSDT",
      "score": 12.5,
      "volume24h": 25000000000,
      "priceChange24h": 3.2,
      "price": 66500,
      "high24h": 67200,
      "low24h": 65800
    }
  ],
  "count": 5,
  "timestamp": "2024-01-15T10:30:00Z"
}
```

### 2. Strategy Analysis

**POST** `/api/strategies/analyze`

Analyzes symbols with all enabled strategies.

**Example:**
```bash
curl -X POST "http://localhost:5000/api/strategies/analyze" \
  -H "Content-Type: application/json" \
  -d '{"symbols": ["BTCUSDT", "ETHUSDT"]}'
```

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

### 3. Performance Metrics

**GET** `/api/performance`

Returns performance metrics for all strategies.

**Example:**
```bash
curl "http://localhost:5000/api/performance"
```

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

### 4. Market Data - Candles

**GET** `/api/market/{symbol}/candles?timeframe=1h&limit=200`

Returns candle data for a symbol.

**Example:**
```bash
curl "http://localhost:5000/api/market/BTCUSDT/candles?timeframe=1h&limit=100"
```

**Response:**
```json
{
  "symbol": "BTCUSDT",
  "timeframe": "1h",
  "candles": [
    {
      "timestamp": "2024-01-15T10:00:00Z",
      "open": 66400,
      "high": 66600,
      "low": 66300,
      "close": 66500,
      "volume": 1250.5
    }
  ],
  "count": 100
}
```

### 5. Market Data - Ticker

**GET** `/api/market/{symbol}/ticker`

Returns current ticker data for a symbol.

**Example:**
```bash
curl "http://localhost:5000/api/market/BTCUSDT/ticker"
```

**Response:**
```json
{
  "symbol": "BTCUSDT",
  "price": "66500",
  "volume24h": 25000000000,
  "priceChange24h": 3.2,
  "high24h": "67200",
  "low24h": "65800",
  "timestamp": "2024-01-15T10:30:00Z"
}
```

## Testing with N8N

### Import Workflows

1. Open N8N at http://localhost:5678
2. Import workflow: `n8n-workflows/symbol-scanner-strategy-analysis.json`
3. Import workflow: `n8n-workflows/performance-optimizer-new-api.json`

### Update Configuration

In each imported workflow, update the HTTP Request node URLs to point to:
```
http://localhost:5000/api/...
```

Or if deployed to production:
```
https://your-trading-engine.com/api/...
```

### Execute Workflows

1. Open a workflow in N8N
2. Click "Execute Workflow"
3. Check the execution results in the workflow log
4. Verify the Trading Engine received the requests in its logs

## Troubleshooting

### Service won't start
- Check if port 5000 is already in use: `lsof -i :5000`
- Review logs for configuration errors
- Ensure `appsettings.json` has all required parameters

### API returns 500 errors
- Check Trading Engine logs for stack traces
- Verify Bitget API connectivity
- Ensure strategies are properly configured

### No symbols returned
- Verify Bitget API is accessible from your network
- Check firewall settings
- Try testing Bitget API directly: `curl https://api.bitget.com/api/v2/spot/market/tickers`

### Strategy analysis returns empty
- Ensure strategies are enabled in `appsettings.json`
- Verify symbols have sufficient historical data
- Check strategy parameter configuration

## Architecture Benefits

✅ **No External API Rate Limits**: All Bitget API calls are centralized in C#  
✅ **Clean Separation**: C# handles data, N8N handles decisions  
✅ **Easy Testing**: Swagger UI for manual API testing  
✅ **Type Safety**: Strong typing with C# models  
✅ **Performance**: Direct Bitget API access, no proxy overhead  
✅ **Maintainability**: Clear API contracts between C# and N8N  

## Next Steps

1. Deploy Trading Engine to production server
2. Configure HTTPS and domain name
3. Add API authentication
4. Set up monitoring and alerting
5. Deploy N8N workflows to production
6. Test end-to-end trading flow with small positions

## API Documentation

Full API documentation is available at:
```
http://localhost:5000/swagger
```

This includes:
- Complete endpoint definitions
- Request/response schemas
- Try-it-out functionality
- Example values
