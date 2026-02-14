using System.Diagnostics;

namespace BitgetApi.TradingEngine.HostedServices;

/// <summary>
/// Hosted service that manages the N8N process lifecycle
/// Automatically starts N8N when the application starts and stops it on shutdown
/// </summary>
public class N8NHostedService : IHostedService, IDisposable
{
    private readonly ILogger<N8NHostedService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private Process? _n8nProcess;
    private bool _isN8NEnabled;
    private string _n8nPort;
    private int _startupDelaySeconds;

    public N8NHostedService(
        ILogger<N8NHostedService> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _isN8NEnabled = configuration.GetValue<bool>("N8N:Enabled", false);
        _n8nPort = configuration.GetValue<string>("N8N:Port") ?? "5678";
        _startupDelaySeconds = configuration.GetValue<int>("N8N:StartupDelaySeconds", 10);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_isN8NEnabled)
        {
            _logger.LogInformation("ℹ️ N8N is disabled in configuration");
            return;
        }

        try
        {
            _logger.LogInformation("🚀 Starting N8N process...");

            // Check if N8N is already running
            if (await IsN8NRunningAsync())
            {
                _logger.LogInformation("✅ N8N is already running on http://localhost:{Port}", _n8nPort);
                return;
            }

            // Check if NPX is available
            if (!IsNpxAvailable())
            {
                _logger.LogError("❌ NPX not found. Please install Node.js and npm first.");
                _logger.LogError("   Download from: https://nodejs.org/");
                return;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = "npx",
                Arguments = _configuration.GetValue<bool>("N8N:UseTunnel", true) 
                    ? "n8n start --tunnel" 
                    : "n8n start",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                Environment =
                {
                    ["N8N_PORT"] = _n8nPort,
                    ["N8N_PROTOCOL"] = "http",
                    ["N8N_HOST"] = "localhost"
                }
            };

            _n8nProcess = new Process { StartInfo = startInfo };

            // Capture output for logging
            _n8nProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    _logger.LogDebug("[N8N] {Output}", e.Data);
                }
            };

            _n8nProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    _logger.LogWarning("[N8N] {Error}", e.Data);
                }
            };

            _n8nProcess.Start();
            _n8nProcess.BeginOutputReadLine();
            _n8nProcess.BeginErrorReadLine();

            _logger.LogInformation("⏳ Waiting {Seconds} seconds for N8N to start...", _startupDelaySeconds);
            await Task.Delay(TimeSpan.FromSeconds(_startupDelaySeconds), cancellationToken);

            // Verify N8N started successfully
            if (await IsN8NRunningAsync())
            {
                _logger.LogInformation("✅ N8N started successfully on http://localhost:{Port}", _n8nPort);
                _logger.LogInformation("🌐 N8N UI: http://localhost:{Port}", _n8nPort);
            }
            else
            {
                _logger.LogError("❌ N8N failed to start. Check if port {Port} is available.", _n8nPort);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error starting N8N process");
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_isN8NEnabled || _n8nProcess == null)
        {
            return;
        }

        try
        {
            _logger.LogInformation("⏹️ Stopping N8N process...");

            if (!_n8nProcess.HasExited)
            {
                _n8nProcess.Kill(entireProcessTree: true);
                await _n8nProcess.WaitForExitAsync(cancellationToken);
            }

            _logger.LogInformation("✅ N8N stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error stopping N8N process");
        }
    }

    private bool IsNpxAvailable()
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "npx",
                Arguments = "--version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            // Wait up to 5 seconds for the process to complete
            process?.WaitForExit(5000);
            return process?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<bool> IsN8NRunningAsync()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            // Use CancellationTokenSource for timeout instead of setting HttpClient.Timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            // N8N health check endpoint - verify root endpoint is accessible
            var response = await httpClient.GetAsync($"http://localhost:{_n8nPort}/", cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        if (_n8nProcess != null && !_n8nProcess.HasExited)
        {
            try
            {
                _n8nProcess.Kill(entireProcessTree: true);
                _n8nProcess.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }
        }
    }
}
