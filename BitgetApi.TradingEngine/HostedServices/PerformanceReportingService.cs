using BitgetApi.TradingEngine.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BitgetApi.TradingEngine.HostedServices;

public class PerformanceReportingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PerformanceReportingService> _logger;
    private readonly TimeSpan _reportingInterval = TimeSpan.FromHours(1);

    public PerformanceReportingService(
        IServiceProvider serviceProvider,
        ILogger<PerformanceReportingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Performance Reporting Service started");

        // Wait 5 minutes before first report to allow for initial data
        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var orchestrator = scope.ServiceProvider.GetRequiredService<StrategyOrchestrator>();

                _logger.LogInformation("Sending performance update to N8N...");
                await orchestrator.SendPerformanceUpdateAsync();
                _logger.LogInformation("Performance update sent");

                await Task.Delay(_reportingInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in performance reporting service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Wait 5 min on error
            }
        }

        _logger.LogInformation("Performance Reporting Service stopped");
    }
}
