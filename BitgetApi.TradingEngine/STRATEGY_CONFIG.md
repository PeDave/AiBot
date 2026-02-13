# Strategy Configuration Guide

This guide explains how to configure and optimize the 4 trading strategies.

## Strategy Overview

| Strategy | Market | Type | Best For |
|----------|--------|------|----------|
| RSI + Volume + EMA | Futures | Momentum | Oversold/overbought bounces with volume confirmation |
| FVG/Liquidity | Futures | Technical | Fair value gap retests near support/resistance |
| Swing | Futures | Mean Reversion | Trading bounces from swing highs/lows |
| Weekly DCA | Spot | Accumulation | Long-term portfolio building |

---

## Strategy 1: RSI + Volume + EMA

### Description
Identifies oversold/overbought conditions with volume spikes while price is relative to 50 EMA.

### Entry Conditions
- **Long**: RSI < 30, Volume > 1.5x average, Price > EMA(50)
- **Short**: RSI > 70, Volume > 1.5x average, Price < EMA(50)

### Parameters

```json
{
  "rsi_period": 14,           // RSI calculation period
  "rsi_oversold": 30.0,       // Oversold threshold (20-35 recommended)
  "rsi_overbought": 70.0,     // Overbought threshold (65-80 recommended)
  "ema_period": 50,           // EMA period (20-100 recommended)
  "volume_multiplier": 1.5,   // Volume spike threshold (1.2-2.0 recommended)
  "tp_percent": 2.0,          // Take profit % (1.5-3.0 recommended)
  "sl_percent": 1.0           // Stop loss % (0.5-1.5 recommended)
}
```

### Optimization Tips
- **Lower win rate (<50%)**: Increase `rsi_oversold` to 35, decrease `rsi_overbought` to 65
- **Too many signals**: Increase `volume_multiplier` to 1.8-2.0
- **Stopped out too often**: Increase `sl_percent` to 1.5%
- **Missing profit**: Increase `tp_percent` to 2.5-3.0%

### Best Markets
- BTC, ETH (high liquidity)
- Major altcoins during volatile periods

---

## Strategy 2: FVG/Liquidity

### Description
Detects Fair Value Gaps (price imbalances) and enters on retests near liquidity zones.

### Entry Conditions
- **Long**: Bullish FVG retest + near support liquidity zone
- **Short**: Bearish FVG retest + near resistance liquidity zone

### Parameters

```json
{
  "fvg_min_gap_percent": 0.5,      // Minimum gap size to consider (0.3-1.0%)
  "liquidity_lookback": 50,         // Candles to analyze for liquidity (30-100)
  "retest_tolerance_percent": 0.2,  // Retest proximity tolerance (0.1-0.5%)
  "tp_multiplier": 1.5,             // TP as multiple of gap size (1.0-2.0)
  "sl_percent": 1.5                 // Stop loss % (1.0-2.0%)
}
```

### Optimization Tips
- **Too sensitive**: Increase `fvg_min_gap_percent` to 0.7-1.0%
- **Missing setups**: Decrease `fvg_min_gap_percent` to 0.3%
- **False signals**: Increase `liquidity_lookback` to 70-100
- **Better R:R**: Increase `tp_multiplier` to 2.0

### Best Markets
- Trending markets with clear imbalances
- Works well on BTC, ETH, SOL

---

## Strategy 3: Swing

### Description
Trades bounces from swing highs/lows with volume confirmation.

### Entry Conditions
- **Long**: Price near swing low + volume spike + upward momentum
- **Short**: Price near swing high + volume spike + downward momentum

### Parameters

```json
{
  "swing_lookback": 20,                // Period to identify swings (10-30)
  "min_swing_distance_percent": 2.0,   // Min distance between swings (1.5-3.0%)
  "volume_multiplier": 1.3,            // Volume confirmation (1.2-1.5)
  "tp_ratio": 1.0,                     // TP as ratio to previous swing (0.8-1.5)
  "sl_ratio": 0.5                      // SL distance below swing (0.3-0.7)
}
```

### Optimization Tips
- **Too many false swings**: Increase `swing_lookback` to 25-30
- **Missing swings**: Decrease `swing_lookback` to 15
- **Better R:R**: Increase `tp_ratio` to 1.2-1.5
- **Reduce stops**: Decrease `sl_ratio` to 0.3-0.4

### Best Markets
- Range-bound markets
- Works on most altcoins during consolidation

---

## Strategy 4: Weekly DCA

### Description
Dollar-cost averaging strategy that increases buy amounts based on market conditions.

### Entry Conditions
- **Daily DCA (2x)**: Price < 200 EMA AND Price < 200 SMA
- **Liquidity Zone DCA (1.5x)**: Price at major support level
- **Weekly DCA (1x)**: Regular weekly accumulation

### Parameters

```json
{
  "base_amount_usd": 10.0,      // Base DCA amount ($5-$50)
  "ema_period": 200,            // EMA period for trend (100-200)
  "sma_period": 200,            // SMA period for trend (100-200)
  "weekly_multiplier": 1.0,     // Weekly DCA multiplier
  "daily_multiplier": 2.0,      // Below 200 multiplier (1.5-3.0)
  "liquidity_multiplier": 1.5   // Liquidity zone multiplier (1.3-2.0)
}
```

### Optimization Tips
- **Increase accumulation rate**: Raise `base_amount_usd`
- **More aggressive dips**: Increase `daily_multiplier` to 2.5-3.0
- **Conservative**: Use only weekly DCA (disable enhanced buying)

### Best Markets
- BTC and ETH for long-term holds
- Blue-chip altcoins (SOL, AVAX, etc.)

---

## Global Trading Settings

In `appsettings.json`:

```json
{
  "Trading": {
    "EnableAutoTrading": false,         // Master switch for live trading
    "BaseDcaAmountUsd": 10.0,          // Base DCA amount
    "MaxPositionsPerSymbol": 1,         // Max concurrent positions per symbol
    "MaxTotalPositions": 8,             // Max total open positions
    "MinConfidenceThreshold": 70.0,     // Min signal confidence to trade (60-80)
    "MaxPositionSizePercent": 5.0,      // Max position size as % of account (3-10)
    "MaxDrawdownPercent": 20.0          // Max drawdown before pausing (15-30)
  }
}
```

### Key Settings

#### EnableAutoTrading
- **false**: Paper trading mode (signals generated but not executed)
- **true**: Live trading (USE WITH CAUTION)

#### MinConfidenceThreshold
- **60-65**: Aggressive (more trades, lower quality)
- **70-75**: Balanced (recommended)
- **75-80**: Conservative (fewer trades, higher quality)

#### MaxPositionSizePercent
- **3-5%**: Conservative risk management
- **5-8%**: Moderate risk
- **8-10%**: Aggressive (higher risk)

---

## Enabling/Disabling Strategies

In `appsettings.json`:

```json
{
  "Strategies": {
    "RSI_Volume_EMA": {
      "Enabled": true,    // Set to false to disable
      "Market": "Futures"
    }
  }
}
```

Restart the application after changing enabled status.

---

## Performance Monitoring

### Key Metrics to Watch

1. **Win Rate**: Target 55-65% for futures strategies
2. **ROI**: Target 10-20% monthly for aggressive, 5-10% for conservative
3. **Max Drawdown**: Keep under -20%
4. **Average Win/Loss Ratio**: Target > 1.5 (wins are 1.5x larger than losses)

### When to Adjust

- **Win rate < 45%**: Strategy parameters too aggressive, tighten entry conditions
- **ROI declining**: Review TP/SL ratios, consider market conditions
- **Drawdown > 20%**: Pause trading, review recent trades, adjust risk
- **Too few trades**: Lower thresholds, check if strategies are enabled

---

## Backtesting Recommendations

Before using live:

1. Enable paper trading mode (`EnableAutoTrading: false`)
2. Run for at least 1 week
3. Monitor signal quality and N8N decisions
4. Review metrics daily
5. Adjust parameters based on results
6. Enable live trading only after consistent positive results

---

## Risk Management

### Position Sizing
- Never risk more than 2-3% of account per trade
- Use leverage conservatively (1-3x recommended)
- DCA amounts should be small relative to portfolio

### Stop Loss Guidelines
- Always use stop losses on futures positions
- Never move stop loss away from entry
- Consider trailing stops for winning positions

### Portfolio Allocation
- 60-70% in spot DCA (BTC/ETH)
- 30-40% for futures strategies
- Keep cash reserve for opportunities

---

## Common Issues

### No Signals Generated
- Check if strategies are enabled
- Verify symbol list is populated
- Review confidence thresholds

### Too Many Signals
- Increase `MinConfidenceThreshold` to 75-80
- Tighten strategy parameters (RSI levels, volume multipliers)

### Poor Performance
- Review recent market conditions (strategies perform differently in trends vs ranges)
- Check if N8N sentiment is working correctly
- Consider disabling underperforming strategies

### Positions Stopped Out Frequently
- Increase stop loss percentages
- Check if market volatility is unusually high
- Consider wider swing lookback periods

---

## Support

For strategy questions or optimization help, see `README.md` or open an issue on GitHub.
