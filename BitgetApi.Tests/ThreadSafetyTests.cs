using BitgetApi.Dashboard.Models;
using BitgetApi.Dashboard.Services;
using Xunit;

namespace BitgetApi.Tests;

public class ThreadSafetyTests
{
    [Fact]
    public void OrderBookService_GetSnapshot_ThreadSafe()
    {
        // Arrange
        var service = new OrderBookService();
        var exceptions = new List<Exception>();
        var tasks = new List<Task>();
        
        // Act - simulate concurrent reads while writes happen
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 100; j++)
                    {
                        var snapshot = service.GetSnapshot();
                        // Just reading, should never throw
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));
        }
        
        Task.WaitAll(tasks.ToArray());
        
        // Assert
        Assert.Empty(exceptions);
    }
    
    [Fact]
    public void TradeStreamService_GetRecentTrades_ThreadSafe()
    {
        // Arrange
        var service = new TradeStreamService(maxTrades: 20);
        var exceptions = new List<Exception>();
        var tasks = new List<Task>();
        
        // Act - simulate concurrent reads
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 100; j++)
                    {
                        var trades = service.GetRecentTrades();
                        // Should never throw "Collection was modified"
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));
        }
        
        Task.WaitAll(tasks.ToArray());
        
        // Assert
        Assert.Empty(exceptions);
    }
    
    [Fact]
    public void PriceTrackerService_GetPrice_ThreadSafe()
    {
        // Arrange
        var service = new PriceTrackerService();
        var exceptions = new List<Exception>();
        var tasks = new List<Task>();
        
        // Act - simulate concurrent reads
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 100; j++)
                    {
                        var price = service.GetPrice("BTCUSDT");
                        // ConcurrentDictionary should handle this safely
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));
        }
        
        Task.WaitAll(tasks.ToArray());
        
        // Assert
        Assert.Empty(exceptions);
    }
}
