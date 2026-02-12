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
            var earnTask = _accountService.GetEarnAccountAsync();
            
            await Task.WhenAll(spotTask, futuresTask, earnTask);
            
            var spot = await spotTask;
            var futures = await futuresTask;
            var earn = await earnTask;
            
            // Calculate Spot balance
            if (spot?.Data != null)
            {
                summary.SpotBalance = spot.Data
                    .Sum(a => decimal.TryParse(a.UsdtValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0);
                
                foreach (var a in spot.Data)
                {
                    if (decimal.TryParse(a.Available, NumberStyles.Any, CultureInfo.InvariantCulture, out var available) && 
                        available > 0 &&
                        decimal.TryParse(a.Frozen, NumberStyles.Any, CultureInfo.InvariantCulture, out var frozen) &&
                        decimal.TryParse(a.UsdtValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var usdtValue))
                    {
                        summary.Assets.Add(new AssetBalance
                        {
                            Coin = a.Coin,
                            Available = available,
                            Frozen = frozen,
                            UsdtValue = usdtValue,
                            Account = "Spot"
                        });
                    }
                }
            }
            
            // Calculate Futures balance
            if (futures?.Data != null)
            {
                summary.FuturesBalance = futures.Data
                    .Sum(a => decimal.TryParse(a.UsdtEquity, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0);
                
                foreach (var a in futures.Data)
                {
                    if (decimal.TryParse(a.Available, NumberStyles.Any, CultureInfo.InvariantCulture, out var available) &&
                        decimal.TryParse(a.Frozen, NumberStyles.Any, CultureInfo.InvariantCulture, out var frozen) &&
                        decimal.TryParse(a.UsdtEquity, NumberStyles.Any, CultureInfo.InvariantCulture, out var usdtEquity) &&
                        usdtEquity > 0)
                    {
                        summary.Assets.Add(new AssetBalance
                        {
                            Coin = a.MarginCoin,
                            Available = available,
                            Frozen = frozen,
                            UsdtValue = usdtEquity,
                            Account = "Futures"
                        });
                    }
                }
            }
            
            // Calculate Earn balance
            if (earn?.Data != null)
            {
                summary.EarnBalance = earn.Data
                    .Sum(a => decimal.TryParse(a.UsdtValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0);
                
                foreach (var a in earn.Data)
                {
                    if (decimal.TryParse(a.Amount, NumberStyles.Any, CultureInfo.InvariantCulture, out var amount) && 
                        amount > 0 &&
                        decimal.TryParse(a.UsdtValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var usdtValue))
                    {
                        summary.Assets.Add(new AssetBalance
                        {
                            Coin = a.Coin,
                            Available = amount,
                            Frozen = 0,
                            UsdtValue = usdtValue,
                            Account = "Earn"
                        });
                    }
                }
            }
            
            summary.TotalBalance = summary.SpotBalance + summary.FuturesBalance + summary.EarnBalance;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching portfolio: {ex.Message}");
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
