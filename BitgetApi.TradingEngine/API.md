# API Documentation

This document describes the N8N webhook endpoints exposed by the Trading Engine.

## Base URL

```
http://localhost:5000/api/n8n
```

For production, use your deployed URL (e.g., `https://trading-engine.yourdomain.com/api/n8n`)

---

## Endpoints

### 1. POST /api/n8n/symbols

Receives symbol recommendations from N8N Symbol Scanner.

#### Request

```http
POST /api/n8n/symbols
Content-Type: application/json

{
  "selectedSymbols": ["BTCUSDT", "ETHUSDT", "SOLUSDT", "AVAXUSDT", "MATICUSDT", "LINKUSDT"],
  "reasoning": {
    "BTCUSDT": "Fixed core holding - Bitcoin",
    "ETHUSDT": "Fixed core holding - Ethereum",
    "SOLUSDT": "High volume, bullish momentum, sentiment: 78%",
    "AVAXUSDT": "Strong volume spike, positive social sentiment",
    "MATICUSDT": "Technical breakout, high trading volume",
    "LINKUSDT": "Consolidation pattern, accumulation phase"
  }
}
```

#### Response

```json
{
  "success": true,
  "message": "Updated 6 symbols",
  "symbols": ["BTCUSDT", "ETHUSDT", "SOLUSDT", "AVAXUSDT", "MATICUSDT", "LINKUSDT"]
}
```

#### Error Response

```json
{
  "success": false,
  "message": "No symbols provided"
}
```

---

### 2. POST /api/n8n/decision

Receives trade decision from N8N Strategy Decision Engine.

#### Request

```http
POST /api/n8n/decision
Content-Type: application/json

{
  "symbol": "BTCUSDT",
  "decision": "EXECUTE",
  "trade": {
    "direction": "LONG",
    "entryPrice": 66500,
    "stopLoss": 65800,
    "takeProfit": 68500,
    "positionSizeUsd": 150,
    "leverage": 3,
    "confidence": 85.2
  },
  "strategyScores": {
    "RSI_Volume_EMA": 72.5,
    "FVG_Liquidity": 88.0,
    "Swing": 75.0
  },
  "reasoning": "3/3 strategies bullish. FVG retest confirmed. Twitter sentiment: 78% bullish."
}
```

#### Response (EXECUTE)

```json
{
  "success": true,
  "message": "Trade executed for BTCUSDT",
  "decision": "EXECUTE",
  "confidence": 85.2
}
```

#### Response (NO_ACTION)

```json
{
  "success": true,
  "message": "No action taken for BTCUSDT",
  "decision": "NO_ACTION",
  "reasoning": "No strategies passed 70% probability threshold"
}
```

#### Error Response

```json
{
  "success": false,
  "message": "Error message here"
}
```

---

### 3. POST /api/n8n/optimize

Receives parameter optimization recommendations from N8N Performance Optimizer.

#### Request

```http
POST /api/n8n/optimize
Content-Type: application/json

{
  "optimizations": [
    {
      "strategy": "RSI_Volume_EMA",
      "parameter": "rsi_oversold",
      "currentValue": 30,
      "suggestedValue": 35,
      "reason": "Win rate drops below RSI 30. Suggest 35 for better accuracy.",
      "confidence": 0.87
    },
    {
      "strategy": "Swing",
      "parameter": "swing_lookback",
      "currentValue": 20,
      "suggestedValue": 25,
      "reason": "Increase lookback for more reliable swing points.",
      "confidence": 0.82
    },
    {
      "strategy": "RSI_Volume_EMA",
      "parameter": "tp_percent",
      "currentValue": 2.0,
      "suggestedValue": 2.5,
      "reason": "Strategy performing well. Can increase take profit target.",
      "confidence": 0.75
    }
  ]
}
```

#### Response

```json
{
  "success": true,
  "message": "Applied 3 optimizations",
  "totalRecommendations": 3,
  "appliedCount": 3
}
```

#### Notes
- Only optimizations with `confidence >= 0.7` are applied
- Parameters are updated immediately
- No restart required

---

## Outgoing Webhooks

These are webhooks that the Trading Engine sends TO N8N.

### 1. Strategy Analysis Webhook

**Endpoint**: `{N8N_BASE_URL}/strategy-analysis`  
**Frequency**: Every 15 minutes per symbol  
**Purpose**: Send strategy signals to N8N for AI decision-making

#### Payload

```json
{
  "symbol": "BTCUSDT",
  "signals": [
    {
      "strategy": "RSI_Volume_EMA",
      "type": "LONG",
      "entryPrice": 66500,
      "stopLoss": 65835,
      "takeProfit": 67830,
      "confidence": 72.5,
      "reason": "RSI oversold (28.5), Volume spike (2.3x), Price above EMA(66300)"
    },
    {
      "strategy": "FVG_Liquidity",
      "type": "LONG",
      "entryPrice": 66500,
      "stopLoss": 65700,
      "takeProfit": 68200,
      "confidence": 88.0,
      "reason": "Bullish FVG retest at 66200-66600, near liquidity zone"
    }
  ],
  "marketData": {
    "price": 66500,
    "volume24h": 1250000000
  }
}
```

#### Expected Response

See "POST /api/n8n/decision" above.

---

### 2. Performance Webhook

**Endpoint**: `{N8N_BASE_URL}/performance`  
**Frequency**: Every hour  
**Purpose**: Send performance metrics for optimization

#### Payload

```json
{
  "strategies": [
    {
      "name": "RSI_Volume_EMA",
      "winRate": 58.0,
      "roi": 8.5,
      "drawdown": -12.0,
      "totalTrades": 45,
      "parameters": {
        "rsi_oversold": 30,
        "rsi_overbought": 70,
        "volume_multiplier": 1.5
      }
    },
    {
      "name": "FVG_Liquidity",
      "winRate": 65.0,
      "roi": 12.3,
      "drawdown": -8.5,
      "totalTrades": 32,
      "parameters": {
        "fvg_min_gap_percent": 0.5,
        "liquidity_lookback": 50
      }
    }
  ],
  "overallPerformance": {
    "totalRoi": 15.5,
    "winRate": 62.0,
    "maxDrawdown": -15.0,
    "totalTrades": 120
  }
}
```

#### Expected Response

See "POST /api/n8n/optimize" above.

---

## Configuration

Set N8N webhook URLs in `appsettings.json`:

```json
{
  "N8N": {
    "WebhookBaseUrl": "https://your-n8n-instance.com/webhook",
    "SymbolScannerWebhook": "/symbol-scanner",
    "StrategyAnalysisWebhook": "/strategy-analysis",
    "PerformanceWebhook": "/performance"
  }
}
```

---

## Authentication

Currently, endpoints are open. For production, implement:

### Option 1: API Key Header

Add to N8N HTTP Request nodes:

```json
{
  "headers": {
    "X-API-Key": "your-secret-key"
  }
}
```

Validate in Trading Engine middleware.

### Option 2: IP Whitelist

Configure firewall to only allow requests from N8N server IP.

### Option 3: HMAC Signature

Implement request signing for secure communication.

---

## Rate Limiting

No rate limiting is currently implemented. Consider adding for production:

- Max 10 requests per minute per endpoint
- Implement exponential backoff in N8N workflows

---

## Error Handling

### HTTP Status Codes

- `200 OK`: Success
- `400 Bad Request`: Invalid request data
- `500 Internal Server Error`: Server error

### Error Response Format

```json
{
  "success": false,
  "message": "Error description",
  "error": "Detailed error message (dev mode only)"
}
```

---

## Testing Endpoints

### Using curl

```bash
# Test symbols endpoint
curl -X POST http://localhost:5000/api/n8n/symbols \
  -H "Content-Type: application/json" \
  -d '{"selectedSymbols":["BTCUSDT","ETHUSDT"],"reasoning":{"BTCUSDT":"Test"}}'

# Test decision endpoint
curl -X POST http://localhost:5000/api/n8n/decision \
  -H "Content-Type: application/json" \
  -d '{
    "symbol":"BTCUSDT",
    "decision":"EXECUTE",
    "trade":{
      "direction":"LONG",
      "entryPrice":66500,
      "stopLoss":65800,
      "takeProfit":68500,
      "positionSizeUsd":150,
      "leverage":3,
      "confidence":85.2
    },
    "strategyScores":{"RSI_Volume_EMA":72.5},
    "reasoning":"Test trade"
  }'

# Test optimize endpoint
curl -X POST http://localhost:5000/api/n8n/optimize \
  -H "Content-Type: application/json" \
  -d '{
    "optimizations":[{
      "strategy":"RSI_Volume_EMA",
      "parameter":"rsi_oversold",
      "currentValue":30,
      "suggestedValue":35,
      "reason":"Test optimization",
      "confidence":0.8
    }]
  }'
```

### Using Postman

1. Import the endpoints above
2. Set `Content-Type: application/json` header
3. Use JSON payloads from examples

---

## Monitoring

### Logs

All API calls are logged. Check logs for:

```
[Information] Updated symbols from N8N: 6 symbols
[Information] Received decision from N8N for BTCUSDT: EXECUTE
[Information] Applied optimization for RSI_Volume_EMA.rsi_oversold: 30 -> 35
```

### Health Check (Future)

Planned endpoint: `GET /health`

```json
{
  "status": "healthy",
  "strategies": 4,
  "openPositions": 3,
  "activeSymbols": 6,
  "lastN8NContact": "2024-02-13T20:30:00Z"
}
```

---

## Support

For API issues or questions, see:
- `README.md` for general documentation
- `N8N_SETUP.md` for N8N configuration
- GitHub Issues for bug reports
