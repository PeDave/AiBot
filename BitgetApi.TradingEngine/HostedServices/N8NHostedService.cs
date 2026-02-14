using System.Diagnostics;
using System.Runtime.InteropServices;

namespace BitgetApi.TradingEngine.HostedServices;

/// <summary>
/// Hosted service that manages the N8N process lifecycle
/// Automatically starts N8N when the application starts and stops it on shutdown
/// </summary>
public class N8NHostedService : IHostedService, IDisposable
{
    private readonly ILogger<N8NHostedService> _logger;
    private readonly IConfiguration _configuration;
    private Process? _n8nProcess;
    private bool _isN8NEnabled;
    private string _n8nPort;
    private int _startupDelaySeconds;
    private string? _npxPath;
    
    private const int HealthCheckTimeoutSeconds = 3;
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(HealthCheckTimeoutSeconds) };

    public N8NHostedService(
        ILogger<N8NHostedService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _isN8NEnabled = configuration.GetValue<bool>("N8N:Enabled", false);
        _n8nPort = configuration.GetValue<string>("N8N:Port", "5678");
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

            // Resolve NPX path
            _npxPath = ResolveNpxPath();
            if (_npxPath == null)
            {
                _logger.LogError("❌ NPX not found. Please install Node.js and npm first.");
                _logger.LogError("   Download from: https://nodejs.org/");
                _logger.LogError("   After installation, restart the application.");
                return;
            }

            _logger.LogInformation("✅ Found NPX at: {NpxPath}", _npxPath);

            var startInfo = new ProcessStartInfo
            {
                FileName = _npxPath,
                Arguments = _configuration.GetValue<bool>("N8N:UseTunnel", true) 
                    ? "n8n start --tunnel" 
                    : "n8n start",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = Environment.CurrentDirectory
            };

            // Set environment variables for N8N
            startInfo.Environment["N8N_PORT"] = _n8nPort;
            startInfo.Environment["N8N_PROTOCOL"] = "http";
            startInfo.Environment["N8N_HOST"] = "localhost";

            // Inherit user PATH environment (important for Windows)
            var pathDirectories = GetPathDirectories();
            
            if (pathDirectories.Any())
            {
                startInfo.Environment["PATH"] = string.Join(Path.PathSeparator, pathDirectories);
                _logger.LogDebug("PATH environment: {Path}", startInfo.Environment["PATH"]);
            }

            _n8nProcess = new Process { StartInfo = startInfo };

            // Capture output for logging
            _n8nProcess.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    _logger.LogDebug("[N8N] {Output}", e.Data);
                    
                    // Log important N8N messages
                    if (e.Data.Contains("Editor is now accessible", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogInformation("🌐 N8N Editor is accessible");
                    }
                }
            };

            _n8nProcess.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    // Only log actual errors, not warnings
                    if (e.Data.Contains("error", StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogWarning("[N8N] {Error}", e.Data);
                    }
                    else
                    {
                        _logger.LogDebug("[N8N] {Message}", e.Data);
                    }
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
                _logger.LogError("   Try running manually: npx n8n start");
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
                
                // Wait up to 5 seconds for graceful shutdown
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
                
                try
                {
                    await _n8nProcess.WaitForExitAsync(timeoutCts.Token);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("⚠️ N8N did not stop gracefully, forced termination");
                }
            }

            _logger.LogInformation("✅ N8N stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error stopping N8N process");
        }
    }

    /// <summary>
    /// Resolves the full path to NPX executable
    /// Checks common installation locations and PATH
    /// </summary>
    private string? ResolveNpxPath()
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var npxName = isWindows ? "npx.cmd" : "npx";

        _logger.LogDebug("Resolving NPX path (OS: {OS})...", isWindows ? "Windows" : "Unix");

        // Try standard PATH lookup first
        var pathFromEnv = FindInPath(npxName);
        if (pathFromEnv != null)
        {
            _logger.LogDebug("Found NPX in PATH: {Path}", pathFromEnv);
            return pathFromEnv;
        }

        // Windows: Check common installation locations
        if (isWindows)
        {
            var commonPaths = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "nodejs", "npx.cmd"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "nodejs", "npx.cmd"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm", "npx.cmd"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "nodejs", "npx.cmd")
            };

            foreach (var path in commonPaths)
            {
                _logger.LogDebug("Checking: {Path}", path);
                if (File.Exists(path))
                {
                    _logger.LogDebug("Found NPX at: {Path}", path);
                    return path;
                }
            }
        }

        // Unix: Check common locations
        if (!isWindows)
        {
            var unixPaths = new[]
            {
                "/usr/local/bin/npx",
                "/usr/bin/npx",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".npm-global", "bin", "npx")
            };

            foreach (var path in unixPaths)
            {
                if (File.Exists(path))
                {
                    _logger.LogDebug("Found NPX at: {Path}", path);
                    return path;
                }
            }
        }

        _logger.LogWarning("NPX not found in common locations");
        return null;
    }

    /// <summary>
    /// Gets all individual PATH directories from system, user, and process environments
    /// </summary>
    private IEnumerable<string> GetPathDirectories()
    {
        var systemPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine);
        var userPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User);
        var processPath = Environment.GetEnvironmentVariable("PATH");
        
        var allPaths = new[] { systemPath, userPath, processPath }
            .Where(p => !string.IsNullOrEmpty(p))
            .SelectMany(p => p!.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .Distinct();
        
        return allPaths;
    }

    /// <summary>
    /// Searches for executable in PATH environment variable
    /// </summary>
    private string? FindInPath(string fileName)
    {
        var paths = GetPathDirectories();

        foreach (var path in paths)
        {
            try
            {
                var fullPath = Path.Combine(path, fileName);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }
            catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
            {
                // Ignore invalid paths - these are expected when PATH contains unusual entries
                _logger.LogDebug("Skipping invalid path {Path} ({ExceptionType}): {Error}", 
                    path, ex.GetType().Name, ex.Message);
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if N8N is already running by attempting to connect to health endpoint
    /// </summary>
    private async Task<bool> IsN8NRunningAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync($"http://localhost:{_n8nPort}/");
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
