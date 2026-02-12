using System.Globalization;

namespace BitgetApi.Dashboard.Web.Services;

public class PortfolioService
{
    private readonly BitgetAccountService _accountService;
    
    public PortfolioService(BitgetAccountService accountService)
    {
        _accountService = accountService;
    }
    
    public async Task<PortfolioSummary> GetPortfolioSummaryAsync()
    {
        var summary = new PortfolioSummary();
        
        try
        {
            // Fetch all account data
            var spotTask = _accountService.GetSpotAccountAsync();
            var futuresTask = _accountService.GetFuturesAccountAsync();
            var futuresBotTask = _accountService.GetFuturesBotAccountAsync();
            var earnTask = _accountService.GetEarnAccountAsync();
            
            await Task.WhenAll(spotTask, futuresTask, futuresBotTask, earnTask);
            
            var spot = await spotTask;
            var futures = await futuresTask;
            var futuresBot = await futuresBotTask;
            var earn = await earnTask;
            
            // ✅ Spot assets - Add each as separate row
            if (spot != null)
            {
                var spotAssets = spot.GetAssets(); // Use helper method
                Console.WriteLine($"Processing {spotAssets.Count} spot assets");
                
                foreach (var asset in spotAssets)
                {
                    if (!decimal.TryParse(asset.Available, NumberStyles.Any, CultureInfo.InvariantCulture, out var available))
                    {
                        Console.WriteLine($"⚠️ Could not parse available amount for {asset.Coin}: '{asset.Available}'");
                        continue;
                    }
                    
                    // Use Frozen or Locked field
                    var frozenStr = asset.Frozen ?? asset.Locked ?? "0";
                    if (!decimal.TryParse(frozenStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var frozen))
                    {
                        frozen = 0;
                    }
                    
                    if (!decimal.TryParse(asset.UsdtValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var usdtValue))
                    {
                        Console.WriteLine($"⚠️ Could not parse USDT value for {asset.Coin}: '{asset.UsdtValue}'");
                        continue;
                    }
                    
                    // Skip assets with zero value
                    if (available <= 0 && frozen <= 0)
                    {
                        Console.WriteLine($"⏩ Skipping {asset.Coin} (zero balance)");
                        continue;
                    }
                    
                    summary.Assets.Add(new AssetBalance
                    {
                        Coin = asset.Coin,
                        Available = available,
                        Frozen = frozen,
                        UsdtValue = usdtValue,
                        Account = "Spot"
                    });
                    
                    summary.SpotBalance += usdtValue;
                    Console.WriteLine($"✅ Added {asset.Coin}: ${usdtValue:N2} (Spot)");
                }
            }
            
            // ✅ Futures assets - Add each as separate row
            if (futures != null)
            {
                var futuresAccounts = futures.GetAccounts(); // Use helper method
                Console.WriteLine($"Processing {futuresAccounts.Count} futures accounts");
                
                foreach (var account in futuresAccounts)
                {
                    if (!decimal.TryParse(account.Available, NumberStyles.Any, CultureInfo.InvariantCulture, out var available))
                    {
                        Console.WriteLine($"⚠️ Could not parse available amount for {account.MarginCoin}: '{account.Available}'");
                        continue;
                    }
                    
                    // Use Frozen or Locked field
                    var frozenStr = account.Frozen ?? account.Locked ?? "0";
                    if (!decimal.TryParse(frozenStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var frozen))
                    {
                        frozen = 0;
                    }
                    
                    if (!decimal.TryParse(account.UsdtEquity, NumberStyles.Any, CultureInfo.InvariantCulture, out var equity))
                    {
                        Console.WriteLine($"⚠️ Could not parse USDT equity for {account.MarginCoin}: '{account.UsdtEquity}'");
                        continue;
                    }
                    
                    if (available <= 0 && frozen <= 0)
                    {
                        Console.WriteLine($"⏩ Skipping {account.MarginCoin} (zero balance)");
                        continue;
                    }
                    
                    summary.Assets.Add(new AssetBalance
                    {
                        Coin = account.MarginCoin,
                        Available = available,
                        Frozen = frozen,
                        UsdtValue = equity,
                        Account = "Futures"
                    });
                    
                    summary.FuturesBalance += equity;
                    Console.WriteLine($"✅ Added {account.MarginCoin}: ${equity:N2} (Futures)");
                }
            }
            
            // ✅ Futures Bot assets - Add each as separate row
            if (futuresBot != null)
            {
                var futuresBotAssets = futuresBot.GetAssets(); // Use helper method
                Console.WriteLine($"Processing {futuresBotAssets.Count} futures bot assets");
                
                foreach (var asset in futuresBotAssets)
                {
                    if (!decimal.TryParse(asset.Available, NumberStyles.Any, CultureInfo.InvariantCulture, out var available))
                    {
                        Console.WriteLine($"⚠️ Could not parse available amount for {asset.Coin}: '{asset.Available}'");
                        continue;
                    }
                    
                    // Use Frozen or Locked field
                    var frozenStr = asset.Frozen ?? asset.Locked ?? "0";
                    if (!decimal.TryParse(frozenStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var frozen))
                    {
                        frozen = 0;
                    }
                    
                    if (!decimal.TryParse(asset.Equity, NumberStyles.Any, CultureInfo.InvariantCulture, out var equity))
                    {
                        Console.WriteLine($"⚠️ Could not parse equity for {asset.Coin}: '{asset.Equity}'");
                        continue;
                    }
                    
                    if (available <= 0 && frozen <= 0)
                    {
                        Console.WriteLine($"⏩ Skipping {asset.Coin} (zero balance)");
                        continue;
                    }
                    
                    summary.Assets.Add(new AssetBalance
                    {
                        Coin = asset.Coin,
                        Available = available,
                        Frozen = frozen,
                        UsdtValue = equity,
                        Account = "Futures Bot"
                    });
                    
                    summary.BotBalance += equity;
                    Console.WriteLine($"✅ Added {asset.Coin}: ${equity:N2} (Futures Bot)");
                }
            }
            
            // ✅ Earn assets - Add each as separate row
            if (earn != null)
            {
                var earnAssets = earn.GetAssets(); // Use helper method
                Console.WriteLine($"Processing {earnAssets.Count} earn assets");
                
                foreach (var asset in earnAssets)
                {
                    if (!decimal.TryParse(asset.Amount, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                    {
                        Console.WriteLine($"⚠️ Could not parse amount for {asset.Coin}: '{asset.Amount}'");
                        continue;
                    }
                    
                    if (!decimal.TryParse(asset.UsdtValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var usdtValue))
                    {
                        Console.WriteLine($"⚠️ Could not parse USDT value for {asset.Coin}: '{asset.UsdtValue}'");
                        continue;
                    }
                    
                    if (amount <= 0)
                    {
                        Console.WriteLine($"⏩ Skipping {asset.Coin} (zero balance)");
                        continue;
                    }
                    
                    summary.Assets.Add(new AssetBalance
                    {
                        Coin = asset.Coin,
                        Available = amount,
                        Frozen = 0,
                        UsdtValue = usdtValue,
                        Account = "Earn"
                    });
                    
                    summary.EarnBalance += usdtValue;
                    Console.WriteLine($"✅ Added {asset.Coin}: ${usdtValue:N2} (Earn)");
                }
            }
            
            summary.TotalBalance = summary.SpotBalance + summary.FuturesBalance + summary.EarnBalance + summary.BotBalance;
            
            // ✅ Sort assets by USD value (descending)
            summary.Assets = summary.Assets.OrderByDescending(a => a.UsdtValue).ToList();
            
            Console.WriteLine($"✅ Portfolio Summary:");
            Console.WriteLine($"   Total: ${summary.TotalBalance:N2}");
            Console.WriteLine($"   - Spot: ${summary.SpotBalance:N2} ({summary.Assets.Count(a => a.Account == "Spot")} assets)");
            Console.WriteLine($"   - Futures: ${summary.FuturesBalance:N2} ({summary.Assets.Count(a => a.Account == "Futures")} assets)");
            Console.WriteLine($"   - Futures Bot: ${summary.BotBalance:N2} ({summary.Assets.Count(a => a.Account == "Futures Bot")} assets)");
            Console.WriteLine($"   - Earn: ${summary.EarnBalance:N2} ({summary.Assets.Count(a => a.Account == "Earn")} assets)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error fetching portfolio: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            summary.HasError = true;
            summary.ErrorMessage = ex.Message;
        }
        
        return summary;
    }

    public async Task<PortfolioSummary> GetMockPortfolioAsync()
    {
        var summary = new PortfolioSummary
        {
            SpotBalance = 1040.62m,
            FuturesBalance = 0m,
            EarnBalance = 0m,
            BotBalance = 0m,
            TotalBalance = 1664.47m
        };

        summary.Assets.Add(new AssetBalance
        {
            Coin = "USDT",
            Account = "Spot",
            Available = 1040.62m,
            Frozen = 0m,
            UsdtValue = 1040.62m
        });

        summary.Assets.Add(new AssetBalance
        {
            Coin = "BTC",
            Account = "Spot",
            Available = 0.00503089m,
            Frozen = 0m,
            UsdtValue = 330.73m
        });

        summary.Assets.Add(new AssetBalance
        {
            Coin = "ETH",
            Account = "Spot",
            Available = 0.10016975m,
            Frozen = 0m,
            UsdtValue = 192.43m
        });

        summary.Assets.Add(new AssetBalance
        {
            Coin = "USDC",
            Account = "Spot",
            Available = 100.05m,
            Frozen = 0m,
            UsdtValue = 100.05m
        });

        return summary;
    }
}

public class PortfolioSummary
{
    public decimal TotalBalance { get; set; }
    public decimal SpotBalance { get; set; }
    public decimal FuturesBalance { get; set; }
    public decimal EarnBalance { get; set; }
    public decimal BotBalance { get; set; } // For future implementation
    public List<AssetBalance> Assets { get; set; } = new();
    public bool HasError { get; set; }
    public string ErrorMessage { get; set; } = "";
}

public class AssetBalance
{
    public string Coin { get; set; } = "";
    public decimal Available { get; set; }
    public decimal Frozen { get; set; }
    public decimal UsdtValue { get; set; }
    public string Account { get; set; } = "";
}
