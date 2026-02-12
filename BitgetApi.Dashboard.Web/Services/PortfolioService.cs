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
                
                summary.Assets.AddRange(spot.Data
                    .Where(a => decimal.TryParse(a.Available, NumberStyles.Any, CultureInfo.InvariantCulture, out var av) && av > 0)
                    .Select(a => new AssetBalance
                    {
                        Coin = a.Coin,
                        Available = decimal.Parse(a.Available, CultureInfo.InvariantCulture),
                        Frozen = decimal.Parse(a.Frozen, CultureInfo.InvariantCulture),
                        UsdtValue = decimal.Parse(a.UsdtValue, CultureInfo.InvariantCulture),
                        Account = "Spot"
                    }));
            }
            
            // Calculate Futures balance
            if (futures?.Data != null)
            {
                summary.FuturesBalance = futures.Data
                    .Sum(a => decimal.TryParse(a.UsdtEquity, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0);
                
                summary.Assets.AddRange(futures.Data
                    .Select(a => new AssetBalance
                    {
                        Coin = a.MarginCoin,
                        Available = decimal.Parse(a.Available, CultureInfo.InvariantCulture),
                        Frozen = decimal.Parse(a.Frozen, CultureInfo.InvariantCulture),
                        UsdtValue = decimal.Parse(a.UsdtEquity, CultureInfo.InvariantCulture),
                        Account = "Futures"
                    }));
            }
            
            // Calculate Earn balance
            if (earn?.Data != null)
            {
                summary.EarnBalance = earn.Data
                    .Sum(a => decimal.TryParse(a.UsdtValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : 0);
                
                summary.Assets.AddRange(earn.Data
                    .Where(a => decimal.TryParse(a.Amount, NumberStyles.Any, CultureInfo.InvariantCulture, out var am) && am > 0)
                    .Select(a => new AssetBalance
                    {
                        Coin = a.Coin,
                        Available = decimal.Parse(a.Amount, CultureInfo.InvariantCulture),
                        Frozen = 0,
                        UsdtValue = decimal.Parse(a.UsdtValue, CultureInfo.InvariantCulture),
                        Account = "Earn"
                    }));
            }
            
            summary.TotalBalance = summary.SpotBalance + summary.FuturesBalance + summary.EarnBalance;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching portfolio: {ex.Message}");
        }
        
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
}

public class AssetBalance
{
    public string Coin { get; set; } = "";
    public decimal Available { get; set; }
    public decimal Frozen { get; set; }
    public decimal UsdtValue { get; set; }
    public string Account { get; set; } = "";
}
