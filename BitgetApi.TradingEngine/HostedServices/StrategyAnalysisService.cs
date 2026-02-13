using BitgetApi.TradingEngine.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BitgetApi.TradingEngine.HostedServices;

public class StrategyAnalysisService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StrategyAnalysisService> _logger;
    private readonly TimeSpan _analysisInterval = TimeSpan.FromMinutes(15);

    public StrategyAnalysisService(
        IServiceProvider serviceProvider,
        ILogger<StrategyAnalysisService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Strategy Analysis Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var orchestrator = scope.ServiceProvider.GetRequiredService<StrategyOrchestrator>();

                _logger.LogInformation("Running strategy analysis...");
                await orchestrator.RunAnalysisAsync();
                _logger.LogInformation("Strategy analysis completed");

                await Task.Delay(_analysisInterval, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in strategy analysis service");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait 1 min on error
            }
        }

        _logger.LogInformation("Strategy Analysis Service stopped");
    }
}
