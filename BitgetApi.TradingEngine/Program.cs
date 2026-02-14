using BitgetApi;
using BitgetApi.Models;
using BitgetApi.TradingEngine.Services;
using BitgetApi.TradingEngine.Trading;
using BitgetApi.TradingEngine.Strategies;
using BitgetApi.TradingEngine.N8N;
using BitgetApi.TradingEngine.HostedServices;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Bitget Trading Engine API",
        Version = "v1",
        Description = "REST API for N8N integration - Symbol scanning, strategy analysis, and performance tracking"
    });
});

// Configure Bitget API Client
var bitgetConfig = builder.Configuration.GetSection("Bitget");
var credentials = new BitgetCredentials
{
    ApiKey = bitgetConfig["ApiKey"] ?? "",
    SecretKey = bitgetConfig["SecretKey"] ?? "",
    Passphrase = bitgetConfig["Passphrase"] ?? ""
};

builder.Services.AddSingleton(credentials);
builder.Services.AddSingleton<BitgetApiClient>(sp => new BitgetApiClient(credentials));

// Configure Database
builder.Services.AddDbContext<TradingDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") 
        ?? "Data Source=trading.db"));

// Register Trading Infrastructure
builder.Services.AddSingleton<BitgetFuturesClient>();
builder.Services.AddSingleton<BitgetSpotClient>();
builder.Services.AddSingleton<PositionManager>();
builder.Services.AddSingleton<RiskManager>();

// Register Strategies
builder.Services.AddSingleton<IStrategy>(sp =>
{
    var config = builder.Configuration.GetSection("Strategies:RSI_Volume_EMA:Parameters");
    var parameters = config.Get<Dictionary<string, object>>();
    return new RsiVolumeEmaStrategy(parameters);
});

builder.Services.AddSingleton<IStrategy>(sp =>
{
    var config = builder.Configuration.GetSection("Strategies:FVG_Liquidity:Parameters");
    var parameters = config.Get<Dictionary<string, object>>();
    return new FvgLiquidityStrategy(parameters);
});

builder.Services.AddSingleton<IStrategy>(sp =>
{
    var config = builder.Configuration.GetSection("Strategies:Swing:Parameters");
    var parameters = config.Get<Dictionary<string, object>>();
    return new SwingStrategy(parameters);
});

builder.Services.AddSingleton<IStrategy>(sp =>
{
    var config = builder.Configuration.GetSection("Strategies:Weekly_DCA:Parameters");
    var parameters = config.Get<Dictionary<string, object>>();
    return new WeeklyDcaStrategy(parameters);
});

// Register LLM Strategy
builder.Services.AddHttpClient<LLMAnalysisStrategy>();
builder.Services.AddSingleton<IStrategy, LLMAnalysisStrategy>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<LLMAnalysisStrategy>>();
    var config = sp.GetRequiredService<IConfiguration>();
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(nameof(LLMAnalysisStrategy));
    return new LLMAnalysisStrategy(logger, config, httpClient);
});

// Register N8N Integration
builder.Services.AddHttpClient<N8NWebhookClient>()
    .ConfigureHttpClient(client =>
    {
        // Reduce timeout from 100s to 30s
        client.Timeout = TimeSpan.FromSeconds(30);
    })
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new SocketsHttpHandler
        {
            // Close connections after 2 minutes of inactivity
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            
            // Limit max connections to prevent accumulation
            MaxConnectionsPerServer = 10,
            
            // Enable automatic decompression
            AutomaticDecompression = System.Net.DecompressionMethods.All,
            
            // Connection lifetime before recycling
            PooledConnectionLifetime = TimeSpan.FromMinutes(5)
        };
    });

// Register Market Data and Analysis Services
builder.Services.AddHttpClient<BitgetMarketDataService>(client =>
{
    client.BaseAddress = new Uri("https://api.bitget.com");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddSingleton<SymbolScanner>();

// Register Altcoin Scanner
builder.Services.AddHttpClient<AltcoinScannerService>(client =>
{
    client.BaseAddress = new Uri("https://api.bitget.com");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddSingleton<AltcoinScannerService>();

// Register Services
builder.Services.AddSingleton<SymbolManager>();
builder.Services.AddScoped<PerformanceTracker>();
builder.Services.AddScoped<StrategyOrchestrator>();

// Register Hosted Services
builder.Services.AddHostedService<StrategyAnalysisService>();
builder.Services.AddHostedService<PerformanceReportingService>();
builder.Services.AddHostedService<N8NHostedService>();
builder.Services.AddHostedService<AltcoinScannerHostedService>();

// Logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TradingDbContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Trading Engine API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("BitgetApi.TradingEngine started");
app.Logger.LogInformation("Auto-trading: {Enabled}", 
    app.Configuration.GetValue<bool>("Trading:EnableAutoTrading", false));

app.Run();
