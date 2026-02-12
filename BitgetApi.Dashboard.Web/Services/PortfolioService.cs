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
            if (spot?.Data != null)
            {
                foreach (var asset in spot.Data)
                {
                    if (!decimal.TryParse(asset.Available, NumberStyles.Any, CultureInfo.InvariantCulture, out var available))
                        continue;
                    
                    if (!decimal.TryParse(asset.Frozen, NumberStyles.Any, CultureInfo.InvariantCulture, out var frozen))
                        frozen = 0;
                    
                    if (!decimal.TryParse(asset.UsdtValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var usdtValue))
                        continue;
                    
                    // Skip assets with zero value
                    if (available <= 0 && frozen <= 0) continue;
                    
                    summary.Assets.Add(new AssetBalance
                    {
                        Coin = asset.Coin,
                        Available = available,
                        Frozen = frozen,
                        UsdtValue = usdtValue,
                        Account = "Spot"
                    });
                    
                    summary.SpotBalance += usdtValue;
                }
            }
            
            // ✅ Futures assets - Add each as separate row
            if (futures?.Data != null)
            {
                foreach (var account in futures.Data)
                {
                    if (!decimal.TryParse(account.Available, NumberStyles.Any, CultureInfo.InvariantCulture, out var available))
                        continue;
                    
                    if (!decimal.TryParse(account.Frozen, NumberStyles.Any, CultureInfo.InvariantCulture, out var frozen))
                        frozen = 0;
                    
                    if (!decimal.TryParse(account.UsdtEquity, NumberStyles.Any, CultureInfo.InvariantCulture, out var equity))
                        continue;
                    
                    if (available <= 0 && frozen <= 0) continue;
                    
                    summary.Assets.Add(new AssetBalance
                    {
                        Coin = account.MarginCoin,
                        Available = available,
                        Frozen = frozen,
                        UsdtValue = equity,
                        Account = "Futures"
                    });
                    
                    summary.FuturesBalance += equity;
                }
            }
            
            // ✅ Futures Bot assets - Add each as separate row
            if (futuresBot?.Data != null)
            {
                foreach (var asset in futuresBot.Data)
                {
                    if (!decimal.TryParse(asset.Available, NumberStyles.Any, CultureInfo.InvariantCulture, out var available))
                        continue;
                    
                    if (!decimal.TryParse(asset.Frozen, NumberStyles.Any, CultureInfo.InvariantCulture, out var frozen))
                        frozen = 0;
                    
                    if (!decimal.TryParse(asset.Equity, NumberStyles.Any, CultureInfo.InvariantCulture, out var equity))
                        continue;
                    
                    if (available <= 0 && frozen <= 0) continue;
                    
                    summary.Assets.Add(new AssetBalance
                    {
                        Coin = asset.Coin,
                        Available = available,
                        Frozen = frozen,
                        UsdtValue = equity,
                        Account = "Futures Bot"
                    });
                    
                    summary.BotBalance += equity;
                }
            }
            
            // ✅ Earn assets - Add each as separate row
            if (earn?.Data != null)
            {
                foreach (var asset in earn.Data)
                {
                    if (!decimal.TryParse(asset.Amount, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount))
                        continue;
                    
                    if (!decimal.TryParse(asset.UsdtValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var usdtValue))
                        continue;
                    
                    if (amount <= 0) continue;
                    
                    summary.Assets.Add(new AssetBalance
                    {
                        Coin = asset.Coin,
                        Available = amount,
                        Frozen = 0,
                        UsdtValue = usdtValue,
                        Account = "Earn"
                    });
                    
                    summary.EarnBalance += usdtValue;
                }
            }
            
            summary.TotalBalance = summary.SpotBalance + summary.FuturesBalance + summary.EarnBalance + summary.BotBalance;
            
            // ✅ Sort assets by USD value (descending)
            summary.Assets = summary.Assets.OrderByDescending(a => a.UsdtValue).ToList();
            
            Console.WriteLine($"✅ Portfolio loaded: Total ${summary.TotalBalance:N2}");
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
