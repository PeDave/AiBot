using BitgetApi.TradingEngine.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BitgetApi.TradingEngine.HostedServices;

public class StrategyAnalysisService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StrategyAnalysisService> _logger;
    private readonly IConfiguration _configuration;
    private readonly N8NHostedService? _n8nService;

    public StrategyAnalysisService(
        IServiceProvider serviceProvider,
        ILogger<StrategyAnalysisService> logger,
        IConfiguration configuration,
        IEnumerable<IHostedService> hostedServices)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
        
        // Find N8N service
        _n8nService = hostedServices.OfType<N8NHostedService>().FirstOrDefault();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Strategy Analysis Service started");

        // Wait for N8N to be ready if enabled
        var n8nEnabled = _configuration.GetValue<bool>("N8N:Enabled", false);
        var useN8N = _configuration.GetValue<bool>("Trading:UseN8NForDecisions", true);

        if (n8nEnabled && useN8N && _n8nService != null)
        {
            _logger.LogInformation("⏳ Waiting for N8N to be ready...");
            
            var maxWaitSeconds = 30;
            var waited = 0;
            
            while (!_n8nService.IsReady && waited < maxWaitSeconds)
            {
                await Task.Delay(1000, stoppingToken);
                waited++;
                
                if (waited % 5 == 0)
                {
                    _logger.LogDebug("Still waiting for N8N... ({Waited}s/{Max}s)", waited, maxWaitSeconds);
                }
            }

            if (_n8nService.IsReady)
            {
                _logger.LogInformation("✅ N8N ready, starting strategy analysis");
            }
            else
            {
                _logger.LogWarning("⚠️ N8N not ready after {Max}s, proceeding anyway (will use fallback if needed)", maxWaitSeconds);
            }
        }

        // Initial delay before first analysis
        var initialDelaySeconds = _configuration.GetValue<int>("StrategyAnalysis:InitialDelaySeconds", 5);
        _logger.LogInformation("⏳ Initial delay: {Seconds} seconds", initialDelaySeconds);
        await Task.Delay(TimeSpan.FromSeconds(initialDelaySeconds), stoppingToken);

        var intervalMinutes = _configuration.GetValue<int>("StrategyAnalysis:IntervalMinutes", 15);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Running strategy analysis...");

                using var scope = _serviceProvider.CreateScope();
                var orchestrator = scope.ServiceProvider.GetRequiredService<StrategyOrchestrator>();

                await orchestrator.RunAnalysisAsync();

                _logger.LogInformation("Strategy analysis completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in strategy analysis");
            }

            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }

        _logger.LogInformation("Strategy Analysis Service stopped");
    }
}
