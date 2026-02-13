# N8N Setup Guide

This guide will help you set up N8N workflows for the Trading Strategy Engine.

## Prerequisites

- N8N instance (self-hosted or cloud)
- OpenAI API key
- Twitter/X API access (optional, for sentiment analysis)
- CoinGecko API access (free tier works)

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

#### Workflow 1: Symbol Scanner
1. Open N8N
2. Click "Workflows" > "Import from File"
3. Select `n8n-workflows/symbol-scanner.json`
4. Click "Import"
5. **Configure**:
   - Update "Send to Trading Engine" node URL to your Trading Engine instance
   - Set your OpenAI credentials in "Analyze Sentiment with OpenAI"
   - Set Twitter credentials in "Fetch Twitter Sentiment" (optional)

#### Workflow 2: Strategy Decision Engine
1. Import `n8n-workflows/strategy-decision-engine.json`
2. **Configure**:
   - Webhook URL will be shown in "Webhook: Receive Signals" node
   - Copy this URL (e.g., `https://your-n8n.com/webhook/strategy-analysis`)
   - Update your Trading Engine `appsettings.json`:
     ```json
     "N8N": {
       "WebhookBaseUrl": "https://your-n8n.com/webhook",
       "StrategyAnalysisWebhook": "/strategy-analysis"
     }
     ```
   - Set OpenAI credentials in AI nodes

#### Workflow 3: Performance Optimizer
1. Import `n8n-workflows/performance-optimizer.json`
2. **Configure**:
   - Copy webhook URL from "Webhook: Receive Performance"
   - Update `appsettings.json`:
     ```json
     "N8N": {
       "PerformanceWebhook": "/performance"
     }
     ```
   - Update "Send to Trading Engine" node with your Trading Engine URL
   - Set OpenAI credentials

### 4. Activate Workflows

1. Open each workflow
2. Click the toggle switch at top right to activate
3. Symbol Scanner will run every 4 hours automatically
4. Other workflows are webhook-triggered

## Testing Workflows

### Test Symbol Scanner
1. Open the workflow
2. Click "Execute Workflow" button
3. Check execution log for any errors
4. Verify Trading Engine received symbols at `/api/n8n/symbols`

### Test Strategy Decision Engine
Send a test request:
```bash
curl -X POST https://your-n8n.com/webhook/strategy-analysis \
  -H "Content-Type: application/json" \
  -d '{
    "symbol": "BTCUSDT",
    "signals": [
      {
        "strategy": "RSI_Volume_EMA",
        "type": "LONG",
        "entryPrice": 66500,
        "stopLoss": 65835,
        "takeProfit": 67830,
        "confidence": 72.5,
        "reason": "RSI oversold, Volume spike"
      }
    ],
    "marketData": {
      "price": 66500,
      "volume24h": 1250000000
    }
  }'
```

### Test Performance Optimizer
```bash
curl -X POST https://your-n8n.com/webhook/performance \
  -H "Content-Type: application/json" \
  -d '{
    "strategies": [
      {
        "name": "RSI_Volume_EMA",
        "winRate": 58.0,
        "roi": 8.5,
        "drawdown": -12.0,
        "totalTrades": 45,
        "parameters": {
          "rsi_oversold": 30,
          "volume_multiplier": 1.5
        }
      }
    ],
    "overallPerformance": {
      "totalRoi": 15.5,
      "winRate": 62.0,
      "maxDrawdown": -15.0
    }
  }'
```

## Workflow Details

### Symbol Scanner
- **Runs**: Every 4 hours
- **Purpose**: Select top 6 crypto symbols based on volume, momentum, and sentiment
- **Output**: Sends selected symbols to Trading Engine

### Strategy Decision Engine
- **Trigger**: Webhook from Trading Engine
- **Purpose**: Analyze strategy signals, fetch sentiment, calculate probabilities
- **Output**: Returns EXECUTE or NO_ACTION decision with trade details

### Performance Optimizer
- **Trigger**: Webhook from Trading Engine (hourly)
- **Purpose**: Analyze strategy metrics and suggest parameter optimizations
- **Output**: Returns optimization recommendations

## Troubleshooting

### Workflow not executing
- Check if workflow is activated (toggle switch)
- Verify credentials are correctly configured
- Check N8N logs for errors

### Webhook not receiving data
- Verify webhook URL in Trading Engine configuration
- Ensure N8N is accessible from Trading Engine
- Check firewall rules

### OpenAI errors
- Verify API key is valid
- Check API usage limits
- Ensure model name is correct (gpt-4 or gpt-3.5-turbo)

### Twitter API errors
- Free tier has limited requests (optional feature)
- Can disable Twitter sentiment if not needed
- Check Bearer Token validity

## Production Deployment

### Security
1. Use HTTPS for all webhooks
2. Add webhook authentication:
   ```json
   "authentication": "headerAuth",
   "headerAuth": {
     "name": "X-API-Key",
     "value": "your-secret-key"
   }
   ```
3. Whitelist Trading Engine IP

### Monitoring
1. Enable N8N workflow execution history
2. Set up error notifications (email/Slack)
3. Monitor OpenAI API costs

### Scaling
1. Use N8N queue mode for high volume
2. Consider rate limiting on webhooks
3. Cache Twitter sentiment data

## Support

For N8N issues:
- Documentation: https://docs.n8n.io
- Community: https://community.n8n.io

For Trading Engine issues:
- See `README.md` in project root
- Check `API.md` for endpoint documentation
