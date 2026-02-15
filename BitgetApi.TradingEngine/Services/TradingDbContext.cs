using BitgetApi.TradingEngine.Models;
using Microsoft.EntityFrameworkCore;

namespace BitgetApi.TradingEngine.Services;

public class TradingDbContext : DbContext
{
    public TradingDbContext(DbContextOptions<TradingDbContext> options) : base(options)
    {
    }

    public DbSet<Position> Positions { get; set; } = null!;
    public DbSet<DcaOrder> DcaOrders { get; set; } = null!;
    public DbSet<StrategyMetrics> StrategyMetrics { get; set; } = null!;
    public DbSet<TradeSignal> TradeSignals { get; set; } = null!;
    public DbSet<N8NDecision> N8NDecisions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Symbol).IsRequired();
            entity.Property(e => e.Strategy).IsRequired();
            entity.Property(e => e.EntryPrice).HasPrecision(18, 8);
            entity.Property(e => e.Size).HasPrecision(18, 8);
            entity.Property(e => e.StopLoss).HasPrecision(18, 8);
            entity.Property(e => e.TakeProfit).HasPrecision(18, 8);
            entity.Property(e => e.ExitPrice).HasPrecision(18, 8);
            entity.Property(e => e.PnL).HasPrecision(18, 8);
            entity.Property(e => e.PnLPercent).HasPrecision(18, 8);
        });

        modelBuilder.Entity<DcaOrder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Symbol).IsRequired();
            entity.Property(e => e.AmountUsd).HasPrecision(18, 8);
            entity.Property(e => e.Price).HasPrecision(18, 8);
            entity.Property(e => e.Quantity).HasPrecision(18, 8);
        });

        modelBuilder.Entity<StrategyMetrics>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.StrategyName).IsRequired();
            
            // Ignore the CurrentParameters property as it's a complex dictionary
            // It's used for runtime data transfer, not for database storage
            entity.Ignore(e => e.CurrentParameters);
        });

        modelBuilder.Entity<TradeSignal>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Symbol).IsRequired();
            entity.Property(e => e.Strategy).IsRequired();
            entity.Property(e => e.Type).IsRequired();
            entity.Property(e => e.EntryPrice).HasPrecision(18, 8);
            entity.Property(e => e.StopLoss).HasPrecision(18, 8);
            entity.Property(e => e.TakeProfit).HasPrecision(18, 8);
            entity.Property(e => e.Confidence).HasPrecision(5, 2);
        });

        modelBuilder.Entity<N8NDecision>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Symbol).IsRequired();
            entity.Property(e => e.Decision).IsRequired();
        });
    }
}
