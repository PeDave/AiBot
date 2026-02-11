# Dashboard Output Example

This file shows sample output from the BitgetApi.Dashboard application.

## Startup Sequence

```
       ____    _   _                    _       ____                  _       _                     
      | __ )  (_) | |_    __ _    ___  | |_    |  _ \    __ _   ___  | |__   | |__     ___     __ _ 
      |  _ \  | | | __|  / _` |  / _ \ | __|   | | | |  / _` | / __| | '_ \  | '_ \   / _ \   / _` |
      | |_) | | | | |_  | (_| | |  __/ | |_    | |_| | | (_| | \__ \ | | | | | |_) | | (_) | | (_| |
      |____/  |_|  \__|  \__, |  \___|  \__|   |____/   \__,_| |___/ |_| |_| |_.__/   \___/   \__,_|
                         |___/                                                                       

Initializing WebSocket connections...
✓ Subscribed to BTCUSDT ticker
✓ Subscribed to ETHUSDT ticker
✓ Subscribed to XRPUSDT ticker
✓ Subscribed to BTCUSDT order book
✓ Subscribed to BTCUSDT trades

Dashboard is ready! Press Q to quit.
```

## Live Dashboard View (After Data Received)

```
┌──────────────────────────────────────────────────────────────────────────────────┐
│                       BITGET LIVE MARKET DASHBOARD v1.0                          │
└──────────────────────────────────────────────────────────────────────────────────┘

┌─PRICES──────────────┐        ┌─ORDER BOOK (BTCUSDT)─────────────────────────────┐
│ ╭─────┬──────┬──────╮│        │ ASKS (SELL)                                      │
│ │ BTC │$67558│▲1.23%││        │ $67,560.00 ████████████░░░░░░░░ 2.3450         │
│ │ ETH │ $3245│▼0.45%││        │ $67,559.50 ██████████░░░░░░░░░░ 1.2340         │
│ │ XRP │  $0.5│▲2.31%││        │ $67,559.00 ████████░░░░░░░░░░░░ 0.8920         │
│ ╰─────┴──────┴──────╯│        │ $67,558.50 ██████░░░░░░░░░░░░░░ 0.5640         │
└─────────────────────┘         │ $67,558.00 ████░░░░░░░░░░░░░░░░ 0.3210         │
                                 │                                                  │
                                 │ SPREAD: $1.00 (0.0015%)                          │
                                 │                                                  │
┌─RECENT TRADES───────┐         │ BIDS (BUY)                                       │
│ ╭──────┬────┬─────╮ │         │ $67,559.00 ██████████████░░░░░░ 3.4560         │
│ │14:32 │BUY │0.120││         │ $67,558.50 ████████████████░░░░ 4.5670         │
│ │14:32 │SELL│0.450││         │ $67,558.00 ██████████████░░░░░░ 3.2340         │
│ │14:31 │BUY │0.234││         │ $67,557.50 ████████████░░░░░░░░ 2.8910         │
│ │14:31 │SELL│1.230││         │ $67,557.00 ██████████░░░░░░░░░░ 2.1230         │
│ │14:30 │BUY │0.089││         └──────────────────────────────────────────────────┘
│ │14:30 │SELL│0.567││         
│ │14:29 │BUY │2.340││         ┌─STATISTICS───────────────────────────────────────┐
│ │14:29 │BUY │0.890││         │ Uptime: 00:15:32  Updates/s: 12.3               │
│ │14:28 │SELL│1.450││         │ Messages: 1,234   Status: Connected             │
│ ╰──────┴────┴─────╯ │         └──────────────────────────────────────────────────┘
└─────────────────────┘

┌──────────────────────────────────────────────────────────────────────────────────┐
│              Q=Quit | R=Reconnect | C=Clear Trades | +/- Depth                   │
└──────────────────────────────────────────────────────────────────────────────────┘
```

## Color Coding (in actual terminal)

- **Green**: 
  - Positive price changes (▲)
  - BUY trades
  - BID (buy) orders
  
- **Red**: 
  - Negative price changes (▼)
  - SELL trades
  - ASK (sell) orders

- **Yellow**: 
  - Panel headers
  - Spread information
  - Alerts (when enabled)

- **Cyan**: 
  - Main header
  - Panel borders
  - Statistics labels

- **Dim/Grey**: 
  - Timestamps
  - Loading states
  - Footer help text

## Update Frequency

The dashboard updates every 100ms by default:
- Prices update as WebSocket ticker messages arrive
- Order book updates in real-time
- Trades stream continuously
- Statistics panel updates every refresh cycle

## Performance Metrics Explained

- **Uptime**: Time since dashboard started (HH:MM:SS)
- **Updates/s**: Average WebSocket messages per second over last 60 seconds
- **Messages**: Total number of WebSocket messages received
- **Status**: WebSocket connection state (Connected/Disconnected)

## Notes

1. The actual output is rendered using Spectre.Console with full color and smooth updates
2. Order book bars scale based on volume (larger volume = longer bar)
3. Trade list auto-scrolls showing most recent trades at top
4. Press Q or Esc to cleanly shut down the dashboard
