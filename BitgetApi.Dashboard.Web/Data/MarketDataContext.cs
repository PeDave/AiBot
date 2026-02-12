using Microsoft.EntityFrameworkCore;
using BitgetApi.Dashboard.Web.Data.Models;

namespace BitgetApi.Dashboard.Web.Data;

public class MarketDataContext : DbContext
{
    public DbSet<CandleData> Candles { get; set; }
    public DbSet<SymbolInfo> Symbols { get; set; }
    public DbSet<TradeHistory> Trades { get; set; }

    public MarketDataContext(DbContextOptions<MarketDataContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CandleData>()
            .HasKey(c => new { c.Symbol, c.Interval, c.Timestamp });

        modelBuilder.Entity<CandleData>()
            .HasIndex(c => new { c.Symbol, c.Interval, c.Timestamp });

        modelBuilder.Entity<SymbolInfo>()
            .HasKey(s => s.Symbol);

        modelBuilder.Entity<TradeHistory>()
            .HasKey(t => new { t.Symbol, t.TradeId });
    }
}
