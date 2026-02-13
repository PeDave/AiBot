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
builder.Services.AddOpenApi();

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

// Register N8N Integration
builder.Services.AddHttpClient<N8NWebhookClient>();

// Register Services
builder.Services.AddSingleton<SymbolManager>();
builder.Services.AddScoped<PerformanceTracker>();
builder.Services.AddScoped<StrategyOrchestrator>();

// Register Hosted Services
builder.Services.AddHostedService<StrategyAnalysisService>();
builder.Services.AddHostedService<PerformanceReportingService>();

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
    app.MapOpenApi();
}

app.UseAuthorization();
app.MapControllers();

app.Logger.LogInformation("BitgetApi.TradingEngine started");
app.Logger.LogInformation("Auto-trading: {Enabled}", 
    app.Configuration.GetValue<bool>("Trading:EnableAutoTrading", false));

app.Run();
