using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BitgetApi.TradingEngine.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BitgetApi.TradingEngine.HostedServices;

public class StrategyAnalysisService : BackgroundService
{
    private readonly ILogger<StrategyAnalysisService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;

    public StrategyAnalysisService(
        ILogger<StrategyAnalysisService> logger,
        IServiceProvider serviceProvider,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Strategy Analysis Service started");

        // ✅ Wait for N8N to be ready if enabled (using HTTP health check)
        var n8nEnabled = _configuration.GetValue<bool>("N8N:Enabled", false);
        var useN8N = _configuration.GetValue<bool>("Trading:UseN8NForDecisions", true);

        if (n8nEnabled && useN8N)
        {
            var n8nPort = _configuration.GetValue<string>("N8N:Port", "5678");
            var n8nUrl = $"http://localhost:{n8nPort}";
            var maxWaitSeconds = _configuration.GetValue<int>("N8N:MaxWaitSeconds", 30);

            _logger.LogInformation("⏳ Waiting for N8N to be ready at {Url}...", n8nUrl);

            var waited = 0;
            var isReady = false;

            while (waited < maxWaitSeconds && !stoppingToken.IsCancellationRequested)
            {
                if (await IsN8NReachableAsync(n8nUrl))
                {
                    _logger.LogInformation("✅ N8N is ready!");
                    isReady = true;
                    break;
                }

                await Task.Delay(1000, stoppingToken);
                waited++;

                if (waited % 5 == 0 && waited > 0)
                {
                    _logger.LogDebug("Still waiting for N8N... ({Waited}s/{Max}s)", waited, maxWaitSeconds);
                }
            }

            if (!isReady)
            {
                _logger.LogWarning("⚠️ N8N not ready after {Max}s, proceeding anyway (will use fallback if needed)", maxWaitSeconds);
            }
        }

        // Initial delay before first analysis
        var initialDelaySeconds = _configuration.GetValue<int>("StrategyAnalysis:InitialDelaySeconds", 5);
        if (initialDelaySeconds > 0)
        {
            _logger.LogInformation("⏳ Initial delay: {Seconds} seconds before first analysis", initialDelaySeconds);
            await Task.Delay(TimeSpan.FromSeconds(initialDelaySeconds), stoppingToken);
        }

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
    }

    /// <summary>
    /// Checks if N8N is reachable via HTTP
    /// </summary>
    private async Task<bool> IsN8NReachableAsync(string url)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            
            using var response = await httpClient.GetAsync(url, cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogTrace(ex, "N8N not reachable at {Url}", url);
            return false;
        }
    }
}
