using System;
using System.Threading;
using System.Threading.Tasks;
using BitgetApi.TradingEngine.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BitgetApi.TradingEngine.HostedServices;

public class AltcoinScannerHostedService : BackgroundService
{
    private readonly ILogger<AltcoinScannerHostedService> _logger;
    private readonly AltcoinScannerService _scannerService;
    private readonly SymbolManager _symbolManager;
    private readonly IConfiguration _configuration;

    public AltcoinScannerHostedService(
        ILogger<AltcoinScannerHostedService> logger,
        AltcoinScannerService scannerService,
        SymbolManager symbolManager,
        IConfiguration configuration)
    {
        _logger = logger;
        _scannerService = scannerService;
        _symbolManager = symbolManager;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enabled = _configuration.GetValue<bool>("AltcoinScanner:Enabled", true);
        if (!enabled)
        {
            _logger.LogInformation("ℹ️ Altcoin Scanner is disabled");
            return;
        }

        _logger.LogInformation("🔍 Altcoin Scanner Service started");

        // Initial delay
        var initialDelaySeconds = _configuration.GetValue<int>("AltcoinScanner:InitialDelaySeconds", 30);
        await Task.Delay(TimeSpan.FromSeconds(initialDelaySeconds), stoppingToken);

        var intervalMinutes = _configuration.GetValue<int>("AltcoinScanner:ScanIntervalMinutes", 60);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("🔍 Running altcoin scan...");

                var topCoins = await _scannerService.ScanAndSelectTopCoinsAsync();
                
                if (topCoins.Count > 0)
                {
                    _symbolManager.UpdateWatchlist(topCoins);
                }

                _logger.LogInformation("✅ Altcoin scan completed. Next scan in {Minutes} minutes.", intervalMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in altcoin scanner");
            }

            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }
}
